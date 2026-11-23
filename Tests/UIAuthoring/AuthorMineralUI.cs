using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorMineralUI {
 static Color ink=new Color(.035f,.08f,.09f,.99f),gold=new Color(.72f,.55f,.29f),paper=new Color(.91f,.85f,.70f);
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Move(string n,float x,float y,float w,float h){var r=N(n).GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static void Border(GameObject g,Color c){var o=g.GetComponent<Outline>();if(o==null)o=g.AddComponent<Outline>();o.effectColor=c;o.effectDistance=new Vector2(2,-2);}
 static void Set(Object o,string f,Object v){var so=new SerializedObject(o);so.FindProperty(f).objectReferenceValue=v;so.ApplyModifiedPropertiesWithoutUndo();}
 static void Label(string n,Transform p,string text,float y,float size){var g=new GameObject(n,typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(p,false);Move(n,0,y,850,38);var t=g.GetComponent<TextMeshProUGUI>();t.font=N("InventoryHeading").GetComponent<TMP_Text>().font;t.text=text;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.color=paper;t.raycastTarget=false;}
 public static void Run(){
 foreach(string n in new[]{"Triangle","Circle","Square"}){var im=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/UI/Crystals/"+n+"-v1.png");im.textureType=TextureImporterType.Default;im.alphaIsTransparency=true;im.mipmapEnabled=false;im.maxTextureSize=512;im.textureCompression=TextureImporterCompression.Uncompressed;im.SaveAndReimport();}
 foreach(string sceneName in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");
 foreach(var graphic in Object.FindObjectsByType<CrystalSymbolGraphic>(FindObjectsInactive.Include,FindObjectsSortMode.None))Set(graphic,"illustration",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/Crystals/"+graphic.Shape+"-v1.png"));
 var inv=Object.FindAnyObjectByType<CrystalInventoryUI>();var card=N("PlayerInventory");var parent=card.transform.parent;
 var backdrop=new GameObject("InventoryBackdrop",typeof(RectTransform),typeof(Image));backdrop.layer=5;backdrop.transform.SetParent(parent,false);var r=(RectTransform)backdrop.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;backdrop.GetComponent<Image>().color=new Color(.01f,.025f,.03f,.82f);card.transform.SetParent(backdrop.transform,false);Set(inv,"panel",backdrop);card.SetActive(true);backdrop.SetActive(false);
 Move("PlayerInventory",0,0,960,520);card.GetComponent<Image>().color=ink;Border(card,gold);
 Move("InventoryHeading",-90,205,700,42);var heading=N("InventoryHeading").GetComponent<TMP_Text>();heading.fontSize=27;heading.alignment=TextAlignmentOptions.MidlineLeft;heading.color=paper;
 Move("CloseInventoryButton",355,205,190,40);Move("CloseInventoryLabel",0,0,186,36);N("CloseInventoryLabel").GetComponent<TMP_Text>().fontSize=15;
 Label("InventorySubtitle",card.transform,"CRISTAIS DA CAVERNA",156,17);
 for(int i=0;i<3;i++){Move("InventorySlot"+i,(i-1)*280,-4,240,260);var slot=N("InventorySlot"+i);slot.GetComponent<Image>().color=paper;Border(slot,gold);Move("InventoryIcon"+i,0,30,160,160);Move("InventoryItem"+i,0,-83,224,62);var t=N("InventoryItem"+i).GetComponent<TMP_Text>();t.fontSize=21;t.color=new Color(.10f,.20f,.20f);}
 Label("InventoryHelp",card.transform,"Minere para encontrar os cristais. Use cada forma no altar da caverna.",-180,18);
 Label("InventoryFooter",card.transform,"I ou Esc para voltar à exploração",-220,15);
 Move("MapCard",0,0,1200,650);N("MapCard").GetComponent<Image>().color=ink;Border(N("MapCard"),gold);
 Move("MapTitle",-200,278,720,45);N("MapTitle").GetComponent<TMP_Text>().alignment=TextAlignmentOptions.MidlineLeft;
 Move("MapSubtitle",-190,237,740,28);N("MapSubtitle").GetComponent<TMP_Text>().text="Sua rota pela ilha • selecione um destino";
 Move("CloseMap",435,278,230,40);Move("CloseMapLabel",0,0,224,36);N("CloseMapLabel").GetComponent<TMP_Text>().fontSize=17;
 Move("MapOcean",-190,-5,740,402);Border(N("MapOcean"),gold);
 Vector2[] pos={new Vector2(.22f,.87f),new Vector2(.21f,.57f),new Vector2(.49f,.59f),new Vector2(.80f,.57f),new Vector2(.65f,.29f)};
 for(int i=0;i<5;i++){Move("MapPin"+IslandMapGraphic.Locations[i],(pos[i].x-.5f)*740,(.5f-pos[i].y)*402,128,42);Border(N("MapPin"+IslandMapGraphic.Locations[i]),new Color(.09f,.18f,.18f));}
 Move("MapDetail",400,-5,300,402);N("MapDetail").GetComponent<Image>().color=new Color(.08f,.16f,.17f);Border(N("MapDetail"),gold);
 Move("LocationPreview",0,110,276,155);Move("LocationTitle",0,5,272,38);Move("LocationInfo",0,-75,264,118);Move("MapTravel",0,-167,270,44);
 Move("MapLegend",-190,-225,740,28);N("MapLegend").GetComponent<TMP_Text>().text="A viagem rápida fica disponível após visitar o lugar.";
 Move("MapObjective",0,-279,1130,64);N("MapObjective").GetComponent<TMP_Text>().fontSize=20;
 EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();Debug.Log("MINERAL UI AUTHORED");EditorApplication.Exit(0);
 }
}
