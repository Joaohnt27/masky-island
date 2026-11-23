using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class AuthorPauseStyle {
 static readonly Color Ink=new Color(.16f,.25f,.24f), Paper=new Color(.98f,.92f,.77f), Gold=new Color(.65f,.43f,.19f);
 static TMP_FontAsset body,title;
 static Texture paper;
 static RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size){var existing=parent.Find(name);if(existing!=null)Object.DestroyImmediate(existing.gameObject);var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static TMP_Text Label(Transform p,string name,string value,float x,float y,float w,float h,int size){var t=Rect(p,name,new Vector2(x,y),new Vector2(w,h)).gameObject.AddComponent<TextMeshProUGUI>();t.text=value;t.font=body;t.fontSize=size;t.color=Ink;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static void Frame(Transform card){var old=card.Find("CoastalPaper");if(old!=null)Object.DestroyImmediate(old.gameObject);var r=Rect(card,"CoastalPaper",Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var image=r.gameObject.AddComponent<RawImage>();image.texture=paper;image.raycastTarget=false;r.SetAsFirstSibling();card.GetComponent<Image>().color=Color.clear;}
 static void StyleButton(Button b,bool primary){var image=b.GetComponent<Image>();image.color=primary?Ink:new Color(.87f,.75f,.53f);var outline=b.GetComponent<Outline>();if(outline==null)outline=b.gameObject.AddComponent<Outline>();outline.effectColor=Gold;outline.effectDistance=new Vector2(2,-2);var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.14f,1.12f,1.05f);colors.selectedColor=colors.highlightedColor;colors.pressedColor=new Color(.8f,.8f,.75f);colors.disabledColor=new Color(.7f,.7f,.7f);colors.fadeDuration=.12f;b.colors=colors;var art=b.GetComponentInChildren<RawImage>(true);if(art==null){var br=(RectTransform)b.transform;art=b.transform.parent.GetComponentsInChildren<RawImage>(true).Where(a=>a.transform.parent==b.transform.parent&&Vector2.Distance(a.rectTransform.anchoredPosition,br.anchoredPosition)<55).OrderBy(a=>Vector2.Distance(a.rectTransform.anchoredPosition,br.anchoredPosition)).FirstOrDefault();if(art!=null){art.transform.SetParent(b.transform,false);art.rectTransform.anchorMin=art.rectTransform.anchorMax=art.rectTransform.pivot=Vector2.one*.5f;art.rectTransform.anchoredPosition=Vector2.zero;art.transform.SetAsFirstSibling();art.raycastTarget=false;}}if(art!=null){image.color=Color.clear;outline.enabled=false;art.color=Color.white;b.targetGraphic=art;}foreach(var t in b.GetComponentsInChildren<TMP_Text>(true)){t.font=body;t.color=primary?Paper:Ink;t.fontSize=22;t.raycastTarget=false;} }
 public static void Run(){
 body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");title=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/Kablammo-Regular-VariableFont_MORF SDF.asset");paper=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/CoastalDialogue-v1.png");
 foreach(string sceneName in new[]{"Beach","House","Island","Cave","Lighthouse"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");var pause=Object.FindAnyObjectByType<PauseController>();var data=new SerializedObject(pause);var panel=(GameObject)data.FindProperty("pausePanel").objectReferenceValue;
 panel.GetComponent<Image>().color=new Color(.025f,.065f,.07f,.73f);var card=panel.transform.Find("Card");Place((RectTransform)card,0,0,1040,440);Frame(card);
 var heading=card.Find("PauseTitle").GetComponent<TMP_Text>();heading.text="Uma pausa\nna jornada";heading.font=title;heading.color=Ink;heading.fontSize=47;Place(heading.rectTransform,-230,48,390,150);
 Label(card,"PauseSubtitle","A ilha espera por você.",-230,-62,350,52,21);
 Label(card,"PauseKeyHint","ESC  ·  voltar à aventura",180,-143,390,36,16);
 string[] names={"ResumeButton","PauseSettingsButton","PauseMainMenuButton"};string[] labels={"Continuar","Configurações","Menu principal"};
 for(int i=0;i<3;i++){var b=card.Find(names[i]).GetComponent<Button>();Place((RectTransform)b.transform,180,88-i*80,370,60);StyleButton(b,i==0);var label=b.GetComponentInChildren<TMP_Text>();label.text=labels[i];Place(label.rectTransform,0,0,350,54);}
 var settings=(SettingsMenuUI)data.FindProperty("settingsUI").objectReferenceValue;var sd=new SerializedObject(settings);sd.FindProperty("font").objectReferenceValue=body;sd.ApplyModifiedPropertiesWithoutUndo();var sp=(GameObject)sd.FindProperty("settingsPanel").objectReferenceValue;
 var bg=sp.transform.Find("Backdrop");bg.GetComponent<Image>().color=new Color(.025f,.065f,.07f,.85f);var sc=sp.transform.Find("SettingsCard");sc.GetComponent<Image>().color=Paper;
 var edge=sc.GetComponent<Outline>();if(edge==null)edge=sc.gameObject.AddComponent<Outline>();edge.effectColor=Gold;edge.effectDistance=new Vector2(3,-3);
 foreach(var t in sp.GetComponentsInChildren<TMP_Text>(true)){t.font=body;t.color=Ink;}
 foreach(var b in sp.GetComponentsInChildren<Button>(true))StyleButton(b,false);
 foreach(var slider in sp.GetComponentsInChildren<Slider>(true)){slider.fillRect.GetComponent<Image>().color=Ink;slider.handleRect.GetComponent<Image>().color=Gold;}
 var controls=(GameObject)sd.FindProperty("pages").GetArrayElementAtIndex(2).objectReferenceValue;foreach(Transform child in controls.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);Label(controls.transform,"ControlGuide","W A S D  —  mover\nClique  —  interagir e se aproximar\n\nI  —  inventário     M  —  mapa     J  —  diário\n\nESC  —  pausar, voltar ou fechar painéis",0,0,840,300,23);
 panel.SetActive(false);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();PauseStyleValidation.Run();
 }
}




