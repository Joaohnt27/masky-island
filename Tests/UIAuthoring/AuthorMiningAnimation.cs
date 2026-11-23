using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class AuthorMiningAnimation {
 public static void Run() {
  const string path="Assets/Art/Characters/Player/PlayerMining-v1.png";
  AssetDatabase.Refresh();
  var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(path));var pixels=tex.GetPixels32();
  int cw=tex.width/6,ch=tex.height/2;var bounds=new RectInt[12];
  for(int r=0;r<2;r++)for(int c=0;c<6;c++) {
   int x0=c*cw,y0=tex.height-(r+1)*ch,minX=cw,minY=ch,maxX=0,maxY=0;
   for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(pixels[(y0+y)*tex.width+x0+x].a>32){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
   bounds[r*6+c]=new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
  }
  float body=(bounds[0].height+bounds[6].height)*.5f;var metadata=new SpriteMetaData[12];
  for(int r=0;r<2;r++)for(int c=0;c<6;c++) {
   int i=r*6+c,x0=c*cw,y0=tex.height-(r+1)*ch;var b=bounds[i];int footMin=cw,footMax=0;
   for(int y=b.y;y<Math.Min(ch,b.y+(int)(body*.10f));y++)for(int x=0;x<cw;x++)if(pixels[(y0+y)*tex.width+x0+x].a>100){footMin=Math.Min(footMin,x);footMax=Math.Max(footMax,x);}
   metadata[i]=new SpriteMetaData{name=(r==0?"MineLeft_":"MineRight_")+c,rect=new Rect(x0,y0,cw,ch),alignment=(int)SpriteAlignment.Custom,pivot=new Vector2((footMin+footMax)*.5f/cw,(b.y+body*.5f)/ch)};
  }
  UnityEngine.Object.DestroyImmediate(tex);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
  importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.spritePixelsPerUnit=body/1.35f;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 0618
  importer.spritesheet=metadata;
#pragma warning restore 0618
  importer.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
  var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Player/Player.controller");var machine=controller.layers[0].stateMachine;
  foreach(string name in new[]{"MineLeft","MineRight"}) {
   var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/"+name+".anim");if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,"Assets/Animations/Player/"+name+".anim");}
   clip.frameRate=9;var frames=sprites.Where(s=>s.name.StartsWith(name+"_")).OrderBy(s=>s.name).ToArray();if(frames.Length!=6)throw new Exception("Missing frames");
   var keys=new ObjectReferenceKeyframe[7];for(int i=0;i<7;i++)keys[i]=new ObjectReferenceKeyframe{time=i/9f,value=frames[i%6]};
   AnimationUtility.SetObjectReferenceCurve(clip,new EditorCurveBinding{path="",type=typeof(SpriteRenderer),propertyName="m_Sprite"},keys);var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
   var state=machine.states.Select(s=>s.state).FirstOrDefault(s=>s.name==name);if(state==null)state=machine.AddState(name);state.motion=clip;EditorUtility.SetDirty(clip);
  }
  EditorUtility.SetDirty(controller);var scene=EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");var player=UnityEngine.Object.FindAnyObjectByType<PlayerSpriteAnimation>();var so=new SerializedObject(player);so.FindProperty("miningSpot").objectReferenceValue=UnityEngine.Object.FindAnyObjectByType<MiningSpot>();so.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();MiningAnimationValidation.Run();
 }
}
