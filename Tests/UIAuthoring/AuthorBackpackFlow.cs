using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorBackpackFlow {
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 public static void Run(){
 EditorSceneManager.OpenScene("Assets/Scenes/Beach.unity");var source=Object.FindAnyObjectByType<DialogueController>();var data=new SerializedObject(source);
 var root=new GameObject("HouseThoughtController",typeof(RectTransform));var rect=(RectTransform)root.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.sizeDelta=Vector2.zero;
 var panel=Object.Instantiate((GameObject)data.FindProperty("dialoguePanel").objectReferenceValue,root.transform,false);panel.name="HouseThoughtPanel";
 var dialogue=root.AddComponent<DialogueController>();Set(dialogue,"dialoguePanel",panel);
 foreach(string field in new[]{"dialogueText","npcNameText","advanceLabel"}){var original=(TMP_Text)data.FindProperty(field).objectReferenceValue;Set(dialogue,field,panel.GetComponentsInChildren<TMP_Text>(true).First(x=>x.name==original.name));}
 foreach(var button in panel.GetComponentsInChildren<Button>(true)){var so=new SerializedObject(button);so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls").ClearArray();so.ApplyModifiedPropertiesWithoutUndo();UnityEventTools.AddPersistentListener(button.onClick,dialogue.NextLine);}
 var template=PrefabUtility.SaveAsPrefabAsset(root,"Assets/Editor/HouseThoughtTemplate.prefab");
 var house=EditorSceneManager.OpenScene("Assets/Scenes/House.unity");var instance=(GameObject)PrefabUtility.InstantiatePrefab(template);PrefabUtility.UnpackPrefabInstance(instance,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
 instance.transform.SetParent(N("GameplayMenusCanvas").transform,false);rect=(RectTransform)instance.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
 dialogue=instance.GetComponent<DialogueController>();var player=Object.FindAnyObjectByType<PlayerClickMovement>();Set(dialogue,"playerMovement",player);Set(Object.FindAnyObjectByType<ScrollInteraction>(),"noticeDialogue",dialogue);
 var story=new SerializedObject(Object.FindAnyObjectByType<StoryLocation>());var panels=story.FindProperty("focusPanels");panels.InsertArrayElementAtIndex(panels.arraySize);panels.GetArrayElementAtIndex(panels.arraySize-1).objectReferenceValue=instance.transform.Find("HouseThoughtPanel").gameObject;story.ApplyModifiedPropertiesWithoutUndo();
 var pickup=new GameObject("HouseBackpack").AddComponent<BackpackPickup>();pickup.transform.position=new Vector3(-3.9f,-1.3f,-1.15f);var visual=new GameObject("BackpackOnFloor",typeof(SpriteRenderer));visual.transform.SetParent(pickup.transform,false);var sprite=visual.GetComponent<SpriteRenderer>();sprite.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icons/Inventory-v1.png");sprite.sortingOrder=4;visual.transform.localScale=Vector3.one*(.95f/sprite.sprite.bounds.size.y);pickup.gameObject.AddComponent<BoxCollider>().size=new Vector3(.85f,.95f,.25f);
 Set(pickup,"player",player.transform);Set(pickup,"dialogue",dialogue);Set(pickup,"backpack",sprite);var pick=new SerializedObject(pickup);pick.FindProperty("interactionDistance").floatValue=.8f;pick.FindProperty("actionLabel").stringValue="Examinar mochila";pick.ApplyModifiedPropertiesWithoutUndo();
 EditorSceneManager.SaveScene(house);
 foreach(var name in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");N("JournalTasks").GetComponent<TMP_Text>().fontSize=16;N("JournalNotes").GetComponent<TMP_Text>().fontSize=15;EditorSceneManager.SaveScene(scene);}
 AssetDatabase.SaveAssets();BackpackFlowValidation.Run();
 }
}
