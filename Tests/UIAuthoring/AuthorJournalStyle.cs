using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorJournalStyle {
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Box(string n,Transform p,float x,float y,float w,float h,Color c){var g=new GameObject(n,typeof(RectTransform),typeof(Image));g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x,y);g.GetComponent<Image>().color=c;g.GetComponent<Image>().raycastTarget=false;g.transform.SetAsFirstSibling();}
 public static void Run(){
 var body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");var title=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/Kablammo-Regular-VariableFont_MORF SDF.asset");
 foreach(var sceneName in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");var cover=N("JournalCover");cover.GetComponent<Image>().color=new Color(.25f,.16f,.10f);
 var border=cover.GetComponent<Outline>();if(border==null)border=cover.AddComponent<Outline>();border.effectColor=new Color(.58f,.39f,.19f);border.effectDistance=new Vector2(4,-4);
 foreach(var text in cover.GetComponentsInChildren<TMP_Text>(true)){text.font=body;text.fontSharedMaterial=body.material;text.fontStyle=FontStyles.Normal;}
 var heading=N("JournalHeading").GetComponent<TMP_Text>();heading.font=title;heading.fontSharedMaterial=title.material;heading.enableAutoSizing=true;heading.fontSizeMin=18;heading.fontSizeMax=30;heading.rectTransform.sizeDelta=new Vector2(760,48);
 foreach(string pageName in new[]{"JournalLeftPage","JournalRightPage"}){var page=N(pageName);page.GetComponent<Image>().color=pageName=="JournalLeftPage"?new Color(.91f,.83f,.66f):new Color(.95f,.88f,.72f);
 var edge=page.AddComponent<Outline>();edge.effectColor=new Color(.57f,.43f,.26f);edge.effectDistance=new Vector2(2,-2);
 Box(pageName+"Rule",page.transform,0,175,440,2,new Color(.52f,.36f,.20f,.65f));
 for(int i=0;i<9;i++)Box(pageName+"Line"+i,page.transform,0,130-i*43,440,1,new Color(.40f,.29f,.16f,.10f));
 }
 N("JournalSpine").GetComponent<Image>().color=new Color(.30f,.19f,.11f);
 for(int i=0;i<12;i++)Box("JournalStitch"+i,cover.transform,0,208-i*39,6,10,new Color(.76f,.59f,.34f));
 foreach(string n in new[]{"JournalTasksHeading","JournalNotesHeading"}){var t=N(n).GetComponent<TMP_Text>();t.fontSize=22;t.color=new Color(.28f,.20f,.12f);}
 var tasks=N("JournalTasks").GetComponent<TMP_Text>();tasks.fontSize=18;tasks.lineSpacing=1;tasks.rectTransform.sizeDelta=new Vector2(440,400);
 var notes=N("JournalNotes").GetComponent<TMP_Text>();notes.fontSize=17;notes.color=new Color(.24f,.23f,.17f);
 var close=N("CloseJournal");close.GetComponent<Image>().color=new Color(.08f,.24f,.25f);var outline=close.AddComponent<Outline>();outline.effectColor=new Color(.77f,.59f,.32f);outline.effectDistance=new Vector2(1,-1);
 var label=N("CloseJournalLabel").GetComponent<TMP_Text>();label.text="FECHAR [J / ESC]";label.fontSize=13;
 var footer=N("JournalFooter").GetComponent<TMP_Text>();footer.text="Cada lembrança deixa uma marca.";footer.fontSize=16;
 EditorSceneManager.SaveScene(scene);
 }AssetDatabase.SaveAssets();JournalStyleValidation.Run();
 }
}
