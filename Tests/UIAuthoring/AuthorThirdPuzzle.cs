using System.Linq;

using TMPro;

using UnityEditor;

using UnityEditor.Events;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.Events;

using UnityEngine.UI;

using Object=UnityEngine.Object;

public static class AuthorThirdPuzzle {

 static TMP_FontAsset font;

 static GameObject Named(string name) {foreach(var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))if(go.name==name)return go;throw new System.Exception(name);}

 static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h) {var go=new GameObject(name,typeof(RectTransform));go.layer=5;go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}

 static TMP_Text Label(string name,Transform parent,string value,float x,float y,float w,float h,float size) {var r=Rect(name,parent,x,y,w,h);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.Normal;t.color=new Color(.96f,.94f,.85f);t.raycastTarget=false;return t;}

 static Button Button(string name,Transform parent,string text,float x,float y,UnityAction action) {var r=Rect(name,parent,x,y,200,58);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.22f,.34f,.37f);var button=r.gameObject.AddComponent<Button>();button.targetGraphic=image;Label("Label",r,text,0,0,190,52,23);UnityEventTools.AddPersistentListener(button.onClick,action);return button;}

 public static void Run() {

  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");

  var interaction=Object.FindAnyObjectByType<ScrollInteraction>(); if(interaction==null)throw new System.Exception("Missing existing Cave note");

  font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

  var canvas=Named("GameplayMenusCanvas").transform;

  var player=Named("Player").GetComponent<PlayerClickMovement>();

  var scrollData=new SerializedObject(interaction);

  var panel=((GameObject)scrollData.FindProperty("scrollPanel").objectReferenceValue).GetComponent<RectTransform>();

  panel.SetParent(canvas,false);panel.SetAsFirstSibling();Place(panel,0,0,750,580);panel.GetComponent<Image>().sprite=null;panel.GetComponent<Image>().color=new Color(.06f,.12f,.15f,.98f);panel.GetComponent<Image>().raycastTarget=true;

  var prompt=(GameObject)scrollData.FindProperty("interactionPrompt").objectReferenceValue;

  prompt.transform.SetParent(canvas,false);var promptText=prompt.GetComponent<TMP_Text>();promptText.text="[E] LER O BILHETE DO ABRIGO";promptText.fontSize=20;

  var pr=prompt.GetComponent<RectTransform>();Place(pr,0,24,600,40);pr.anchorMin=pr.anchorMax=pr.pivot=new Vector2(.5f,0);prompt.SetActive(false);

  var title=panel.Find("ScrollTitle").GetComponent<TMP_Text>();Style(title,0,235,700,50,32);title.text="O BILHETE DO ABRIGO";title.color=new Color(.96f,.77f,.4f);

  var clue=panel.Find("ScrollText").GetComponent<TMP_Text>();string opening=clue.text;Style(clue,0,115,680,155,22);

  var close=panel.Find("CloseButton").GetComponent<Button>();Place(close.GetComponent<RectTransform>(),0,-250,200,50);Style(close.GetComponentInChildren<TMP_Text>(),0,0,190,45,22);

  var puzzle=panel.gameObject.AddComponent<SymbolSequencePuzzle>();

  var lighthouse=Button("LighthouseSymbolButton",panel,"FAROL",-220,10,puzzle.SelectLighthouse);

  var boat=Button("BoatSymbolButton",panel,"BARCO",0,10,puzzle.SelectBoat);

  var fish=Button("FishSymbolButton",panel,"PEIXE",220,10,puzzle.SelectFish);

  var feedback=Label("ReturnPuzzleFeedback",panel,"",0,-120,690,165,22);



  var data=new SerializedObject(puzzle);var sol=data.FindProperty("solution");sol.arraySize=3;

  sol.GetArrayElementAtIndex(0).enumValueIndex=3;sol.GetArrayElementAtIndex(1).enumValueIndex=4;sol.GetArrayElementAtIndex(2).enumValueIndex=5;

  Set(data,"clueText",clue);data.FindProperty("openingClue").stringValue=opening;data.FindProperty("unlockedClue").stringValue="Parti da costa a bordo de um barco.\nSegui um peixe pelas águas escuras.\nVoltei ao farol para devolver sua luz.";

  data.FindProperty("memory").enumValueIndex=2;data.FindProperty("memoryText").stringValue="Você voltou para terminar o que começou.";

  data.FindProperty("requiresPreviousMemory").boolValue=true;data.FindProperty("previousMemory").enumValueIndex=1;

  data.FindProperty("nextStepText").stringValue="Converse com o estranho sobre sua volta.";

  Set(data,"sunButton",boat);Set(data,"moonButton",fish);Set(data,"starButton",lighthouse);Set(data,"feedbackText",feedback);data.ApplyModifiedPropertiesWithoutUndo();

  data=new SerializedObject(interaction);Set(data,"player",player.transform);data.FindProperty("interactionDistance").floatValue=1.6f;Set(data,"playerMovement",player);Set(data,"scrollPanel",panel.gameObject);Set(data,"interactionPrompt",prompt);data.ApplyModifiedPropertiesWithoutUndo();

  panel.gameObject.SetActive(false);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Debug.Log("THIRD PUZZLE AUTHORED");

 }

 static void Place(RectTransform r,float x,float y,float w,float h) {r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}

 static void Style(TMP_Text t,float x,float y,float w,float h,float size) {Place(t.rectTransform,x,y,w,h);t.font=font;t.fontSharedMaterial=font.material;t.fontStyle=FontStyles.Normal;t.margin=Vector4.zero;t.fontSize=size;t.enableAutoSizing=false;t.textWrappingMode=TextWrappingModes.Normal;t.color=new Color(.96f,.94f,.85f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;}

 static void Set(SerializedObject data,string name,Object obj)=>data.FindProperty(name).objectReferenceValue=obj;

}

