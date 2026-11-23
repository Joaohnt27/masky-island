using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorLighthouseQte {
 static TMP_FontAsset font;
 static void Set(Object o,string name,Object v){var s=new SerializedObject(o);s.FindProperty(name).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static RectTransform Rect(Transform p,string name,float x,float y,float w,float h){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);Place(r,x,y,w,h);return r;}
 static TMP_Text Text(Transform p,string name,string value,float x,float y,float w,float h,int size){var t=Rect(p,name,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=value;t.color=new Color(.11f,.23f,.23f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static Button Button(Transform p,string name,string value,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.12f,.27f,.27f);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;var text=Text(r,"Label",value,0,0,w-12,h-4,18);text.color=new Color(.97f,.91f,.75f);r.gameObject.AddComponent<ButtonSound>();return b;}
 static Image Picture(Transform p,string name,Sprite sprite,float x,float y,float w,float h){var r=Rect(p,name,x,y,w,h);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}

 public static void Run(){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Lighthouse.unity");font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");
 var lamp=Object.FindAnyObjectByType<LighthouseInteraction>();var data=new SerializedObject(lamp);var panel=(GameObject)data.FindProperty("panel").objectReferenceValue;var root=(RectTransform)panel.transform;while(root.childCount>0)Object.DestroyImmediate(root.GetChild(0).gameObject);
 root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;root.SetAsFirstSibling();root.GetComponent<Image>().color=new Color(.02f,.04f,.04f,.8f);
 var board=Rect(root,"LighthouseQtePaper",0,0,1100,620);var paper=board.gameObject.AddComponent<Image>();paper.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/DialoguePaper-v2.png");paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=3;
 Text(board,"LighthouseTitle","A ÚLTIMA LUZ",0,258,850,50,32);
 var stages=new TMP_Text[3];for(int i=0;i<3;i++)stages[i]=Text(board,"LighthouseStage"+i,"",-335+i*335,185,320,60,18);
 var image=Rect(board,"LighthouseMechanismArt",-310,-8,255,290).gameObject.AddComponent<RawImage>();image.texture=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/Backgrounds/Story/05-farol.png");image.uvRect=new Rect(.4f,.45f,.25f,.53f);image.raycastTarget=false;var edge=image.gameObject.AddComponent<Outline>();edge.effectColor=new Color(.55f,.36f,.17f);edge.effectDistance=new Vector2(3,-3);
 var status=Text(board,"LighthouseStatus","",180,105,500,68,24);
 var help=Text(board,"LighthouseHelp","Clique em AJUSTAR ou pressione Espaço\nquando o indicador entrar na faixa dourada.",180,40,490,55,17);
 var timing=Rect(board,"LighthouseTiming",180,-22,440,30);timing.gameObject.AddComponent<Image>().color=new Color(.13f,.24f,.24f);
 var target=Rect(timing,"LighthouseTarget",0,0,100,28);target.gameObject.AddComponent<Image>().color=new Color(.94f,.67f,.23f);
 var cursor=Rect(timing,"LighthouseCursor",0,0,6,44);cursor.gameObject.AddComponent<Image>().color=new Color(.98f,.94f,.8f);var needleEdge=cursor.gameObject.AddComponent<Outline>();needleEdge.effectColor=new Color(.35f,.22f,.1f);needleEdge.effectDistance=new Vector2(1,-1);
 var strike=Button(board,"LighthouseAdjust","AJUSTAR  [ESPAÇO]",180,-89,350,46);var activate=Button(board,"ActivateLighthouse","PREPARAR MECANISMO",180,-89,350,46);UnityEventTools.AddPersistentListener(activate.onClick,lamp.Activate);
 var time=Text(board,"LighthouseTimeLabel","",180,-140,440,30,15);
 var bar=Rect(board,"QteTime",180,-168,440,9);bar.gameObject.AddComponent<Image>().color=new Color(.13f,.24f,.24f);var slider=bar.gameObject.AddComponent<Slider>();slider.interactable=false;slider.transition=Selectable.Transition.None;slider.minValue=0;slider.maxValue=1;
 var fill=Rect(bar,"Fill",0,0,440,9);fill.gameObject.AddComponent<Image>().color=new Color(.8f,.57f,.24f);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=fill.offsetMax=Vector2.zero;slider.fillRect=fill;
 var feedback=Text(board,"LighthouseFeedback","",-60,-238,850,58,17);feedback.textWrappingMode=TextWrappingModes.Normal;
 var close=Button(board,"CloseLighthouse","VOLTAR  [ESC]",390,-279,200,36);UnityEventTools.AddPersistentListener(close.onClick,lamp.Close);
 Set(lamp,"helpText",help);Set(lamp,"statusText",status);Set(lamp,"activateButton",activate);Set(lamp,"closeButton",close);Set(lamp,"qteProgress",slider);Set(lamp,"strikeButton",strike);Set(lamp,"cursor",cursor);Set(lamp,"target",target);Set(lamp,"timing",timing.gameObject);Set(lamp,"feedbackText",feedback);Set(lamp,"timeText",time);
 data=new SerializedObject(lamp);data.FindProperty("stepWindow").floatValue=8;var labels=data.FindProperty("stageLabels");labels.arraySize=3;for(int i=0;i<3;i++)labels.GetArrayElementAtIndex(i).objectReferenceValue=stages[i];data.ApplyModifiedPropertiesWithoutUndo();panel.SetActive(false);
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();LighthouseQteValidation.Run();
 }
}

