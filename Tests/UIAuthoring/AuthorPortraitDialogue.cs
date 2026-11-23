using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class AuthorPortraitDialogue {
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 public static void Run(){
 const string path="Assets/Art/UI/DialoguePortraits-v1.png";AssetDatabase.Refresh();var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes(path));var pixels=texture.GetPixels32();var metadata=new SpriteMetaData[2];int cw=texture.width/2;
 for(int i=0;i<2;i++){int minX=cw,minY=texture.height,maxX=0,maxY=0;for(int y=0;y<texture.height;y++)for(int x=0;x<cw;x++)if(pixels[y*texture.width+i*cw+x].a>32){minX=Mathf.Min(minX,x);minY=Mathf.Min(minY,y);maxX=Mathf.Max(maxX,x);maxY=Mathf.Max(maxY,y);}metadata[i]=new SpriteMetaData{name=i==0?"PlayerDialoguePortrait":"StrangerDialoguePortrait",rect=new Rect(i*cw+minX,minY,maxX-minX+1,maxY-minY+1),alignment=(int)SpriteAlignment.BottomCenter,pivot=new Vector2(.5f,0)};}
 Object.DestroyImmediate(texture);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;
#pragma warning disable 0618
 importer.spritesheet=metadata;
#pragma warning restore 0618
 importer.SaveAndReimport();var portraits=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
 const string framePath="Assets/Art/UI/DialoguePaper-v2.png";var fi=(TextureImporter)AssetImporter.GetAtPath(framePath);fi.textureType=TextureImporterType.Sprite;fi.spriteImportMode=SpriteImportMode.Single;fi.spriteBorder=new Vector4(400,300,400,160);fi.spritePixelsPerUnit=100;fi.mipmapEnabled=false;fi.maxTextureSize=4096;fi.textureCompression=TextureImporterCompression.Uncompressed;fi.SaveAndReimport();var frame=AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
 foreach(var name in new[]{"Beach","House","Cave","Island"}){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var d=Object.FindAnyObjectByType<DialogueController>();var s=new SerializedObject(d);var panel=(GameObject)s.FindProperty("dialoguePanel").objectReferenceValue;var card=panel.transform.Find("CoastalDialogueCard");var portrait=(Image)s.FindProperty("speakerPortrait").objectReferenceValue;
 portrait.transform.SetParent(panel.transform,false);Place(portrait.rectTransform,405,390,320,370);portrait.rectTransform.anchorMin=portrait.rectTransform.anchorMax=new Vector2(.5f,0);portrait.preserveAspect=true;portrait.raycastTarget=false;portrait.transform.SetAsFirstSibling();
 Place((RectTransform)card,0,145,1200,230);((RectTransform)card).anchorMin=((RectTransform)card).anchorMax=new Vector2(.5f,0);
 var old=card.GetComponent<RawImage>();if(old!=null)Object.DestroyImmediate(old);var paper=card.GetComponent<Image>();if(paper==null)paper=card.gameObject.AddComponent<Image>();paper.sprite=frame;paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=3;paper.raycastTarget=false;
 var body=(TMP_Text)s.FindProperty("dialogueText").objectReferenceValue;Place(body.rectTransform,-5,2,870,94);body.fontSize=24;body.fontSizeMin=20;body.fontSizeMax=24;body.enableAutoSizing=true;body.alignment=TextAlignmentOptions.MidlineLeft;
 var title=(TMP_Text)s.FindProperty("npcNameText").objectReferenceValue;Place(title.rectTransform,-155,73,570,34);title.fontSize=25;title.fontSizeMax=25;title.color=new Color(.42f,.24f,.12f);
 var advance=(TMP_Text)s.FindProperty("advanceLabel").objectReferenceValue;var button=advance.GetComponentInParent<Button>(true);Place((RectTransform)button.transform,285,-72,310,34);Place(advance.rectTransform,0,0,300,30);advance.fontSize=15;advance.fontSizeMax=15;
 s.FindProperty("playerPortrait").objectReferenceValue=portraits.Single(p=>p.name=="PlayerDialoguePortrait");s.FindProperty("npcPortrait").objectReferenceValue=portraits.Single(p=>p.name=="StrangerDialoguePortrait");portrait.sprite=(Sprite)s.FindProperty(name=="Island"?"npcPortrait":"playerPortrait").objectReferenceValue;s.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();PortraitDialogueValidation.Run();
 }
}

