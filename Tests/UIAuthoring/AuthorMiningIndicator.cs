using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorMiningIndicator {
 static TMP_FontAsset font;
 static void Set(Object o,string name,Object v){var s=new SerializedObject(o);s.FindProperty(name).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);Place(r,x,y,w,h);return r;}
 static TMP_Text Text(Transform p,string name,string value,float x,float y,float w,float h,int size){var t=Rect(p,name,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=value;t.color=new Color(.11f,.23f,.23f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Button Button(Transform p,string name,string value,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.12f,.27f,.27f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var text=Text(r,"Label",value,0,0,w-12,h-4,18);text.color=new Color(.97f,.91f,.75f);r.gameObject.AddComponent<ButtonSound>();return b;}
 static Image Picture(Transform p,string name,Sprite sprite,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}

 public static void Run(){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
 font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");
 var ui=Object.FindAnyObjectByType<MiningUI>();var s=new SerializedObject(ui);var panel=(GameObject)s.FindProperty("worldMiningPanel").objectReferenceValue;var rect=(RectTransform)panel.transform;
 rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(0,24);rect.sizeDelta=new Vector2(620,150);
 var paper=panel.GetComponent<Image>();paper.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/DialoguePaper-v2.png");paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=5;paper.color=Color.white;paper.raycastTarget=false;
 foreach(var outline in panel.GetComponents<Outline>())Object.DestroyImmediate(outline);
 var old=panel.transform.Find("MiningIndicatorIcon");if(old!=null)Object.DestroyImmediate(old.gameObject);old=panel.transform.Find("MiningIndicatorTitle");if(old!=null)Object.DestroyImmediate(old.gameObject);
 Picture(panel.transform,"MiningIndicatorIcon",AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tools/Pickaxe-v1.png"),-250,0,78,90);
 var heading=Text(panel.transform,"MiningIndicatorTitle","MINERAÇÃO",-25,42,350,30,23);heading.font=font;
 var text=(TMP_Text)s.FindProperty("worldMiningText").objectReferenceValue;Place(text.rectTransform,-25,7,370,30);text.font=font;text.fontSize=17;text.enableAutoSizing=false;text.color=new Color(.16f,.27f,.25f);text.alignment=TextAlignmentOptions.Center;text.textWrappingMode=TextWrappingModes.Normal;
 var progress=(Slider)s.FindProperty("worldProgress").objectReferenceValue;Place((RectTransform)progress.transform,-25,-28,350,16);progress.transition=Selectable.Transition.None;var track=progress.GetComponent<Image>();track.color=new Color(.11f,.25f,.25f);track.raycastTarget=false;var fill=progress.fillRect.GetComponent<Image>();fill.color=new Color(.89f,.62f,.22f);fill.raycastTarget=false;
 var cancel=(Button)s.FindProperty("worldCancel").objectReferenceValue;Place((RectTransform)cancel.transform,227,-16,114,36);cancel.GetComponent<Image>().color=new Color(.12f,.27f,.27f);var colors=cancel.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.88f,.63f);colors.pressedColor=new Color(.75f,.68f,.51f);cancel.colors=colors;
 var label=cancel.GetComponentInChildren<TMP_Text>(true);Place(label.rectTransform,0,0,108,32);label.font=font;label.fontSize=13;label.color=new Color(.98f,.92f,.75f);label.text="CANCELAR";
 panel.SetActive(false);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();MiningIndicatorValidation.Run();
 }
}

