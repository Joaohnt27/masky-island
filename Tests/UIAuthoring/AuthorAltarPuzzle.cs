using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorAltarPuzzle {
 static void Move(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 public static void Run(){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");var ui=Object.FindAnyObjectByType<MiningUI>();var data=new SerializedObject(ui);data.FindProperty("altarMode").boolValue=true;data.ApplyModifiedPropertiesWithoutUndo();
 var panel=(GameObject)data.FindProperty("miningPanel").objectReferenceValue;var root=(RectTransform)panel.transform;Move(root,0,0,1280,720);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.sizeDelta=Vector2.zero;panel.GetComponent<Image>().color=Color.clear;panel.GetComponent<Image>().raycastTarget=false;
 foreach(Transform child in panel.transform)child.gameObject.SetActive(false);
 var tray=new GameObject("AltarCrystalTray",typeof(RectTransform),typeof(Image));tray.transform.SetParent(root,false);Move((RectTransform)tray.transform,0,-215,700,260);tray.GetComponent<Image>().color=new Color(.025f,.075f,.09f,.95f);tray.GetComponent<Image>().raycastTarget=true;tray.transform.SetAsFirstSibling();
 var cards=(GameObject)data.FindProperty("crystalGroup").objectReferenceValue;cards.SetActive(true);Move((RectTransform)cards.transform,0,-210,550,120);
 foreach(var button in cards.GetComponentsInChildren<Button>(true)){var rect=(RectTransform)button.transform;Move(rect,rect.anchoredPosition.x,0,166,120);var surface=button.transform.Find("CardSurface");if(surface!=null)Move((RectTransform)surface,0,0,162,116);var symbol=button.GetComponentInChildren<CrystalSymbolGraphic>(true);Move(symbol.rectTransform,0,20,65,65);var label=button.GetComponentInChildren<TMP_Text>(true);Move(label.rectTransform,0,-35,156,40);label.fontSize=16;}
 var sockets=(GameObject)data.FindProperty("socketGroup").objectReferenceValue;sockets.SetActive(true);Move((RectTransform)sockets.transform,0,0,1280,720);
 var buttons=data.FindProperty("socketButtons");var layout=panel.AddComponent<AltarPuzzleLayout>();var ld=new SerializedObject(layout);var slots=ld.FindProperty("slots");var points=ld.FindProperty("worldPositions");slots.arraySize=points.arraySize=3;float[] x={-.55f,.30f,1.18f};
 for(int i=0;i<3;i++){var b=(Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue;Move((RectTransform)b.transform,0,0,58,58);var image=b.GetComponent<Image>();image.sprite=null;image.color=new Color(.96f,.77f,.4f,.15f);image.raycastTarget=true;var edge=b.gameObject.GetComponent<Outline>();if(edge==null)edge=b.gameObject.AddComponent<Outline>();edge.effectColor=new Color(.96f,.77f,.4f,.9f);edge.effectDistance=new Vector2(1,-1);foreach(var t in b.GetComponentsInChildren<TMP_Text>(true))t.gameObject.SetActive(false);var g=b.GetComponentInChildren<CrystalSymbolGraphic>(true);Move(g.rectTransform,0,0,48,48);slots.GetArrayElementAtIndex(i).objectReferenceValue=b.transform;points.GetArrayElementAtIndex(i).vector3Value=new Vector3(x[i],1.68f,-1);}
 var feedback=(TMP_Text)data.FindProperty("resultText").objectReferenceValue;feedback.gameObject.SetActive(true);ld.FindProperty("feedback").objectReferenceValue=feedback;ld.FindProperty("socketGroup").objectReferenceValue=sockets;ld.ApplyModifiedPropertiesWithoutUndo();
 var instructions=(TMP_Text)data.FindProperty("instructions").objectReferenceValue;instructions.gameObject.SetActive(true);Move(instructions.rectTransform,0,-115,670,48);instructions.fontSize=20;
 var closeLabel=(TMP_Text)data.FindProperty("closeLabel").objectReferenceValue;var close=closeLabel.GetComponentInParent<Button>(true);close.gameObject.SetActive(true);Move((RectTransform)close.transform,470,-305,210,40);Move(closeLabel.rectTransform,0,0,204,36);closeLabel.fontSize=14;
 var complete=(GameObject)data.FindProperty("completionGroup").objectReferenceValue;Move((RectTransform)complete.transform,0,-210,550,120);foreach(var t in complete.GetComponentsInChildren<TMP_Text>(true))t.gameObject.SetActive(false);foreach(var g in complete.GetComponentsInChildren<CrystalSymbolGraphic>(true)){var r=g.rectTransform;Move(r,r.anchoredPosition.x,0,85,85);}
 panel.SetActive(false);EditorSceneManager.SaveScene(scene);Debug.Log("ALTAR PUZZLE AUTHORED");AltarPuzzleValidation.Run();
 }
}


