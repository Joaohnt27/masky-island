using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorMapAudio {
 static TMP_FontAsset font;
 static Color ink=new Color(.035f,.075f,.09f,.98f), gold=new Color(.96f,.77f,.4f);
 static GameObject Named(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Array(Object o,string f,Object[] values){var s=new SerializedObject(o);var a=s.FindProperty(f);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=values[i];s.ApplyModifiedPropertiesWithoutUndo();}
 static RectTransform Rect(string n,Transform p,float x,float y,float w,float h){var g=new GameObject(n,typeof(RectTransform));g.layer=5;g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
 static Image Box(string n,Transform p,float x,float y,float w,float h,Color c){var v=Rect(n,p,x,y,w,h).gameObject.AddComponent<Image>();v.color=c;return v;}
 static TMP_Text Text(string n,Transform p,string v,float x,float y,float w,float h,float size){var t=Rect(n,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=v;t.fontSize=size;t.color=new Color(.96f,.94f,.85f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Button Button(string n,Transform p,string text,float x,float y,float w,float h){var b=Box(n,p,x,y,w,h,new Color(.18f,.32f,.33f)).gameObject.AddComponent<Button>();b.targetGraphic=b.GetComponent<Image>();Text(n+"Label",b.transform,text,0,0,w-8,h-4,20);return b;}
 public static void Run(){
 font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
 string[] arts={"01-praia","02-casa","03-centro","04-caverna","05-farol"};
 foreach(string name in IslandMapGraphic.Locations){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
 var canvas=Named("GameplayMenusCanvas").transform;var player=Object.FindAnyObjectByType<PlayerClickMovement>();var story=Object.FindAnyObjectByType<StoryLocation>();
 var host=new GameObject("IslandMapController");var map=host.AddComponent<IslandMapUI>();
 var open=Button("OpenMap",canvas,"MAPA [M]",0,0,130,48);var r=(RectTransform)open.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one;r.anchoredPosition=new Vector2(-12,-16);
 var panel=Box("IslandMapPanel",canvas,0,0,1280,720,new Color(.01f,.025f,.035f,.85f));r=panel.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.sizeDelta=Vector2.zero;
 var group=panel.gameObject.AddComponent<CanvasGroup>();
 var card=Box("MapCard",panel.transform,0,0,1120,620,ink);
 Text("MapTitle",card.transform,"MAPA DA ILHA",-320,263,430,45,32).color=gold;
 Text("MapSubtitle",card.transform,"Explore a pé. Volte aos lugares visitados pelo mapa.",-170,225,730,30,18);
 var close=Button("CloseMap",card.transform,"FECHAR [M / ESC]",405,266,250,42);
 var ocean=Box("MapOcean",card.transform,-170,-8,700,380,new Color(.075f,.17f,.20f));
 var land=Rect("IslandDrawing",ocean.transform,0,0,680,370).gameObject.AddComponent<IslandMapGraphic>();land.raycastTarget=false;
 Text("Compass",ocean.transform,"N\n↑",-310,135,40,65,23).color=gold;
 var pins=new Image[5];var labels=new TMP_Text[5];
 for(int i=0;i<5;i++){var pt=IslandMapGraphic.Points[i];var b=Button("MapPin"+IslandMapGraphic.Locations[i],ocean.transform,"",pt.x*680,pt.y*370,136,58);pins[i]=b.GetComponent<Image>();labels[i]=b.GetComponentInChildren<TMP_Text>();UnityEventTools.AddStringPersistentListener(b.onClick,map.SelectLocation,IslandMapGraphic.Locations[i]);}
 var detail=Box("MapDetail",card.transform,372,-8,300,380,new Color(.065f,.125f,.14f));
 var preview=Rect("LocationPreview",detail.transform,0,100,276,155).gameObject.AddComponent<RawImage>();preview.raycastTarget=false;
 var title=Text("LocationTitle",detail.transform,"",0,-2,270,35,25);title.color=gold;
 var info=Text("LocationInfo",detail.transform,"",0,-77,270,112,18);
 var travel=Button("MapTravel",detail.transform,"VIAJAR",0,-158,270,44);
 var objective=Text("MapObjective",card.transform,"",0,-257,1060,78,20);
 Text("MapLegend",card.transform,"Selecione um lugar para ver os detalhes e a disponibilidade de viagem.",-170,-216,700,28,15);
 Set(map,"panel",panel.gameObject);Set(map,"fade",group);Set(map,"player",player);Set(map,"story",story);Set(map,"openButton",open);Set(map,"travelButton",travel);Set(map,"objective",objective);Set(map,"detailTitle",title);Set(map,"detailText",info);Set(map,"travelLabel",travel.GetComponentInChildren<TMP_Text>());Set(map,"preview",preview);
 Array(map,"pins",pins);Array(map,"pinLabels",labels);Array(map,"previews",arts.Select(a=>AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/Backgrounds/Story/"+a+".png")).ToArray());
 UnityEventTools.AddPersistentListener(open.onClick,map.Toggle);UnityEventTools.AddPersistentListener(close.onClick,map.Close);UnityEventTools.AddPersistentListener(travel.onClick,map.Travel);
 var ss=new SerializedObject(story);var focus=ss.FindProperty("focusPanels");focus.InsertArrayElementAtIndex(focus.arraySize);focus.GetArrayElementAtIndex(focus.arraySize-1).objectReferenceValue=panel.gameObject;ss.ApplyModifiedPropertiesWithoutUndo();panel.gameObject.SetActive(false);
 var audio=host.AddComponent<GameplayAudioFeedback>();Set(audio,"player",player);if(name=="Cave")Set(audio,"mining",Object.FindAnyObjectByType<MiningSpot>());
 Named("StoryControls").GetComponent<TMP_Text>().text="W A S D  mover    •    Clique para interagir    •    I  inventário    •    M  mapa    •    Esc  pausa";
 Sounds();EditorSceneManager.SaveScene(scene);
 }
 foreach(string name in new[]{"MainMenu","Credits","Ending"}){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");Sounds();if(name=="Ending"&&Object.FindAnyObjectByType<AudioListener>()==null)new GameObject("EndingAudioListener",typeof(AudioListener));EditorSceneManager.SaveScene(scene);}
 AssetDatabase.SaveAssets();Debug.Log("MAP AUDIO AUTHORED");EditorApplication.Exit(0);
 }
 static void Sounds(){foreach(var b in Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(b.GetComponent<ButtonSound>()==null)b.gameObject.AddComponent<ButtonSound>();}
}
