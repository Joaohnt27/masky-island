using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class AuthorDialogueStyle {
 static void Set(SerializedObject s,string name,Object value){s.FindProperty(name).objectReferenceValue=value;}
 public static void Run(){
 var body=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");var heading=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/Kablammo-Regular-VariableFont_MORF SDF.asset");
 EditorSceneManager.OpenScene("Assets/Scenes/Beach.unity");var source=Object.FindAnyObjectByType<DialogueController>();var sourceData=new SerializedObject(source);var sourcePanel=(GameObject)sourceData.FindProperty("dialoguePanel").objectReferenceValue;var template=Object.Instantiate(sourcePanel.transform.Find("CoastalDialogueCard").gameObject);var preview=EditorSceneManager.NewPreviewScene();UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(template,preview);template.hideFlags=HideFlags.HideAndDontSave;
 foreach(string name in new[]{"Beach","House","Cave","Island"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");foreach(var d in Object.FindObjectsByType<DialogueController>(FindObjectsInactive.Include)){
 var s=new SerializedObject(d);var panel=(GameObject)s.FindProperty("dialoguePanel").objectReferenceValue;
 if(name=="Island"){
 panel.transform.SetParent(Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include).First(c=>c.name=="GameplayMenusCanvas").transform,false);panel.transform.SetAsFirstSibling();foreach(Transform child in panel.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
 var r=(RectTransform)panel.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
 var image=panel.GetComponent<Image>();if(image!=null){image.sprite=null;image.color=new Color(.025f,.05f,.06f,.12f);}
 var card=Object.Instantiate(template,panel.transform,false);card.hideFlags=HideFlags.None;card.name="CoastalDialogueCard";card.SetActive(true);
 var text=card.transform.Find("IntroductionText").GetComponent<TMP_Text>();var speaker=card.transform.Find("IntroductionSpeaker").GetComponent<TMP_Text>();var button=card.GetComponentInChildren<Button>(true);button.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddPersistentListener(button.onClick,d.NextLine);
 Set(s,"dialogueText",text);Set(s,"npcNameText",speaker);Set(s,"advanceLabel",button.GetComponentInChildren<TMP_Text>(true));
 }
 var dialogueText=(TMP_Text)s.FindProperty("dialogueText").objectReferenceValue;var speakerText=(TMP_Text)s.FindProperty("npcNameText").objectReferenceValue;var label=(TMP_Text)s.FindProperty("advanceLabel").objectReferenceValue;
 dialogueText.font=body;dialogueText.fontSize=25;dialogueText.enableAutoSizing=true;dialogueText.fontSizeMin=21;dialogueText.fontSizeMax=25;dialogueText.overflowMode=TextOverflowModes.Overflow;dialogueText.rectTransform.sizeDelta=new Vector2(730,120);dialogueText.rectTransform.anchoredPosition=new Vector2(dialogueText.rectTransform.anchoredPosition.x,0);
 speakerText.font=heading;speakerText.fontStyle=FontStyles.Normal;speakerText.fontSize=23;speakerText.enableAutoSizing=true;speakerText.fontSizeMin=16;speakerText.fontSizeMax=23;speakerText.rectTransform.sizeDelta=new Vector2(570,35);
 if(label!=null){label.font=body;label.fontSize=16;label.enableAutoSizing=true;label.fontSizeMin=12;label.fontSizeMax=16;}
 var portrait=panel.GetComponentsInChildren<Image>(true).FirstOrDefault(i=>i.name=="PlayerPortrait");
 var player=Object.FindAnyObjectByType<PlayerClickMovement>();Set(s,"speakerPortrait",portrait);Set(s,"playerPortrait",player.GetComponent<SpriteRenderer>().sprite);
 if(name=="Island"){var npc=Object.FindAnyObjectByType<NPCInteraction>();var sprite=npc.GetComponentInParent<SpriteRenderer>();if(sprite==null)throw new System.Exception("NPC portrait unavailable");Set(s,"npcPortrait",sprite.sprite);if(portrait!=null)portrait.sprite=sprite.sprite;}
 s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(d);panel.SetActive(false);
 }
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 EditorSceneManager.ClosePreviewScene(preview);AssetDatabase.SaveAssets();DialogueStyleValidation.Run();
 }
}




