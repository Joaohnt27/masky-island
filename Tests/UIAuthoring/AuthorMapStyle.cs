using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorMapStyle {
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static Color paper=new Color(.88f,.79f,.60f),ink=new Color(.16f,.24f,.22f),wood=new Color(.29f,.19f,.11f),teal=new Color(.09f,.26f,.26f);
 static void Move(string n,float x,float y,float w,float h){var r=(RectTransform)N(n).transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static void Box(string n,Transform p,float x,float y,float w,float h,Color c){var g=new GameObject(n,typeof(RectTransform),typeof(Image));g.transform.SetParent(p,false);Move(n,x,y,w,h);g.GetComponent<Image>().color=c;g.GetComponent<Image>().raycastTarget=false;g.transform.SetAsFirstSibling();}
 public static void Run(){
 var body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");
 var title=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/Kablammo-Regular-VariableFont_MORF SDF.asset");
 foreach(var name in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var card=N("MapCard");card.GetComponent<Image>().color=paper;var border=card.GetComponent<Outline>();border.effectColor=wood;border.effectDistance=new Vector2(5,-5);
 foreach(var t in card.GetComponentsInChildren<TMP_Text>(true)){t.font=body;t.fontSharedMaterial=body.material;t.color=ink;t.fontStyle=FontStyles.Normal;t.enableAutoSizing=true;t.fontSizeMin=14;t.fontSizeMax=18;}
 var heading=N("MapTitle").GetComponent<TMP_Text>();heading.font=title;heading.fontSharedMaterial=title.material;heading.text="Mapa da Ilha";heading.fontSizeMin=heading.fontSizeMax=38;heading.color=wood;
 Move("MapTitle",-200,276,720,56);Move("MapSubtitle",-190,234,740,28);var sub=N("MapSubtitle").GetComponent<TMP_Text>();sub.text="Escolha um lugar e continue sua jornada";sub.alignment=TextAlignmentOptions.MidlineLeft;sub.fontSizeMax=16;
 Box("MapHeaderRule",card.transform,0,211,1120,2,new Color(.55f,.39f,.20f));Box("MapObjectivePaper",card.transform,0,-277,1130,70,new Color(.94f,.86f,.69f));
 N("MapDetail").GetComponent<Image>().color=new Color(.94f,.86f,.69f);N("MapDetail").GetComponent<Outline>().effectColor=wood;
 var loc=N("LocationTitle").GetComponent<TMP_Text>();loc.fontSizeMin=loc.fontSizeMax=25;loc.color=wood;Move("LocationTitle",0,4,272,40);
 Move("LocationInfo",0,-72,258,111);N("LocationInfo").GetComponent<TMP_Text>().fontSizeMax=17;
 foreach(string button in new[]{"MapTravel","CloseMap"}){N(button).GetComponent<Image>().color=teal;foreach(var t in N(button).GetComponentsInChildren<TMP_Text>(true)){t.color=new Color(.98f,.92f,.76f);t.fontSizeMax=16;}}
 N("MapLegend").GetComponent<TMP_Text>().fontSizeMax=14;
 foreach(var place in IslandMapGraphic.Locations){var pin=N("MapPin"+place);foreach(var t in pin.GetComponentsInChildren<TMP_Text>(true)){t.fontSizeMin=14;t.fontSizeMax=16;}pin.GetComponent<Outline>().effectColor=wood;}
 N("MapObjective").GetComponent<TMP_Text>().fontSizeMax=18;
 EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();MapStyleValidation.Run();
 }
}
