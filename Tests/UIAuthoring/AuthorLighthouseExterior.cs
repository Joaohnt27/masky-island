using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorLighthouseExterior {
 static TMP_FontAsset font;
 static void Set(Object o,string name,Object v){var s=new SerializedObject(o);s.FindProperty(name).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);Place(r,x,y,w,h);return r;}
 static TMP_Text Text(Transform p,string name,string value,float x,float y,float w,float h,int size){var t=Rect(p,name,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=value;t.color=new Color(.11f,.23f,.23f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Button Button(Transform p,string name,string value,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.12f,.27f,.27f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var text=Text(r,"Label",value,0,0,w-12,h-4,18);text.color=new Color(.97f,.91f,.75f);r.gameObject.AddComponent<ButtonSound>();return b;}
 static Image Picture(Transform p,string name,Sprite sprite,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}

 static void Str(Object o,string field,string value){var s=new SerializedObject(o);s.FindProperty(field).stringValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Array(Object o,string field,Object[] values){var s=new SerializedObject(o);var a=s.FindProperty(field);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo();}
 static Sprite Import(string path){var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.spritePixelsPerUnit=100;t.maxTextureSize=2048;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
 static void Interaction(Interactable item,PlayerClickMovement player,Vector3 at,Vector3 approach,Vector3 size,string label){item.transform.position=at;var p=new GameObject("Approach").transform;p.SetParent(item.transform);p.position=approach;Set(item,"player",player.transform);Set(item,"approachPoint",p);Str(item,"actionLabel",label);var s=new SerializedObject(item);s.FindProperty("interactionDistance").floatValue=2.6f;s.ApplyModifiedPropertiesWithoutUndo();var box=item.gameObject.AddComponent<BoxCollider>();box.size=size;box.isTrigger=true;}
 public static void Run(){
 AssetDatabase.Refresh();font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");
 var bg=Import("Assets/Art/Backgrounds/Story/06-farol-exterior.png");var padlock=Import("Assets/Art/UI/LighthousePadlock-v1.png");
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse.unity");
 var lamp=Object.FindAnyObjectByType<LighthouseInteraction>();var ls=new SerializedObject(lamp);((GameObject)ls.FindProperty("panel").objectReferenceValue).SetActive(false);((GameObject)ls.FindProperty("beacon").objectReferenceValue).SetActive(false);lamp.gameObject.SetActive(false);
 var background=GameObject.Find("StoryBackground").GetComponent<SpriteRenderer>();background.sprite=bg;background.transform.localScale=Vector3.one*(10/bg.bounds.size.y);
 var player=Object.FindAnyObjectByType<PlayerClickMovement>();player.transform.position=new Vector3(-3,-3.3f,-1);var ps=new SerializedObject(player);ps.FindProperty("walkMin").vector2Value=new Vector2(-7.4f,-4.1f);ps.FindProperty("walkMax").vector2Value=new Vector2(7.4f,-2.15f);ps.ApplyModifiedPropertiesWithoutUndo();
 var story=Object.FindAnyObjectByType<StoryLocation>();var st=new SerializedObject(story);st.FindProperty("arrivals").arraySize=0;st.ApplyModifiedPropertiesWithoutUndo();
 var exit=Object.FindAnyObjectByType<DoorInteraction>();exit.transform.position=new Vector3(-6.2f,-3,-1);var es=new SerializedObject(exit);((Transform)es.FindProperty("approachPoint").objectReferenceValue).position=new Vector3(-5.8f,-3,-1);es.ApplyModifiedPropertiesWithoutUndo();var exitLabel=exit.GetComponentInChildren<TMP_Text>();if(exitLabel!=null)exitLabel.transform.position=new Vector3(-5,-2.8f,-1.15f);
 var gate=new GameObject("LighthouseGate").AddComponent<LighthouseGate>();Interaction(gate,player,new Vector3(3.1f,-.2f,-1),new Vector3(3.1f,-2.2f,-1),new Vector3(1.85f,3.2f,.3f),"Examinar a porta do farol");Set(gate,"movement",player);
 var plaque=new GameObject("LighthouseInscription").AddComponent<LighthouseInscription>();Interaction(plaque,player,new Vector3(.7f,-.35f,-1),new Vector3(.8f,-2.2f,-1),new Vector3(1.3f,.9f,.3f),"Ler a inscrição");Set(plaque,"gate",gate);Set(plaque,"movement",player);
 var lockWorld=new GameObject("DoorPadlock",typeof(SpriteRenderer));lockWorld.transform.position=new Vector3(3.7f,-.75f,-1.2f);lockWorld.transform.localScale=Vector3.one*.075f;lockWorld.GetComponent<SpriteRenderer>().sprite=padlock;lockWorld.GetComponent<SpriteRenderer>().sortingOrder=1;Set(gate,"worldLock",lockWorld);
 var canvas=Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include).First(c=>c.name=="GameplayMenusCanvas");var root=Rect(canvas.transform,"LighthouseLockPanel",0,0,0,0);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();root.gameObject.AddComponent<Image>().color=new Color(.02f,.04f,.04f,.8f);
 var board=Rect(root,"LockPaper",0,0,1100,640);var paper=board.gameObject.AddComponent<Image>();paper.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/DialoguePaper-v2.png");paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=3;
 Text(board,"LockTitle","A PORTA DO FAROL",0,265,850,45,30);
 Text(board,"InscriptionTitle","INSCRIÇÃO DO GUARDIÃO",-260,195,420,35,22);
 var clue=Text(board,"LockClue","",-260,15,400,250,22);clue.textWrappingMode=TextWrappingModes.Normal;
 var shapes=new[]{GameManager.CrystalId.Square,GameManager.CrystalId.Circle,GameManager.CrystalId.Triangle};for(int i=0;i<3;i++){var g=Rect(board,"ClueSymbol"+i,-350+i*90,138,48,48).gameObject.AddComponent<CrystalSymbolGraphic>();g.Shape=shapes[i];g.color=new Color(.3f,.4f,.36f);g.raycastTarget=false;}
 Picture(board,"PadlockIllustration",padlock,260,20,430,430);
 var digitLabels=new TMP_Text[3];var up=new Button[3];var down=new Button[3];float[] xs={187,255,321};
 for(int i=0;i<3;i++){digitLabels[i]=Text(board,"LockDigit"+i,"0",xs[i],-42,50,62,38);digitLabels[i].color=new Color(1,.91f,.68f);up[i]=Button(board,"LockPlus"+i,"+",xs[i],8,44,30);down[i]=Button(board,"LockMinus"+i,"-",xs[i],-96,44,30);}
 var keyStatus=Text(board,"LockKeyStatus","",260,225,410,30,18);
 var read=Button(board,"ReadInscription","LER INSCRIÇÃO",-260,-153,300,42);var key=Button(board,"UseLighthouseKey","USAR CHAVE",260,-208,300,42);
 var unlock=Button(board,"UnlockLighthouse","DESTRANCAR",260,-264,300,42);var enter=Button(board,"EnterLighthouse","ENTRAR NO FAROL",260,-264,300,42);
 var close=Button(board,"CloseLighthouseLock","VOLTAR  [ESC]",-360,-270,200,36);
 var feedback=Text(board,"LockFeedback","",-250,-213,430,66,16);feedback.textWrappingMode=TextWrappingModes.Normal;
 Set(gate,"panel",root.gameObject);Set(gate,"clue",clue);Set(gate,"feedback",feedback);Set(gate,"keyStatus",keyStatus);Array(gate,"digits",digitLabels);Array(gate,"increase",up);Array(gate,"decrease",down);Set(gate,"readButton",read);Set(gate,"keyButton",key);Set(gate,"unlockButton",unlock);Set(gate,"enterButton",enter);Set(gate,"closeButton",close);
 st=new SerializedObject(story);var focus=st.FindProperty("focusPanels");focus.InsertArrayElementAtIndex(focus.arraySize);focus.GetArrayElementAtIndex(focus.arraySize-1).objectReferenceValue=root.gameObject;st.ApplyModifiedPropertiesWithoutUndo();root.gameObject.SetActive(false);
 EditorSceneManager.SaveScene(scene,"Assets/Scenes/LighthouseExterior.unity");
 foreach(var name in new[]{"Beach","House","Island","Cave","Lighthouse","LighthouseExterior"}){
 scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
 foreach(var door in Object.FindObjectsByType<DoorInteraction>()){
 if(name=="Island"&&door.SceneName=="Lighthouse")Str(door,"sceneName","LighthouseExterior");
 if(name=="Lighthouse"){Str(door,"sceneName","LighthouseExterior");Str(door,"actionLabel","Sair do farol");var label=door.GetComponentInChildren<TMP_Text>();if(label!=null)label.text="Saída  ›";}
 }
 var map=Object.FindAnyObjectByType<IslandMapUI>();var ms=new SerializedObject(map);ms.FindProperty("previews").GetArrayElementAtIndex(4).objectReferenceValue=bg.texture;ms.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 if(!EditorBuildSettings.scenes.Any(x=>x.path=="Assets/Scenes/LighthouseExterior.unity"))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene("Assets/Scenes/LighthouseExterior.unity",true)}).ToArray();
 AssetDatabase.SaveAssets();LighthouseExteriorValidation.Run();
 }
}

