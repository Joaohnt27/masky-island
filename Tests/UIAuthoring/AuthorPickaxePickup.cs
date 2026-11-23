using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class AuthorPickaxePickup {
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 public static void Run(){
 EditorSceneManager.OpenScene("Assets/Scenes/House.unity");var template=PrefabUtility.SaveAsPrefabAsset(Object.FindAnyObjectByType<DialogueController>().gameObject,"Assets/Editor/CaveThoughtTemplate.prefab");
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");var root=(GameObject)PrefabUtility.InstantiatePrefab(template);PrefabUtility.UnpackPrefabInstance(root,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);root.name="CaveThoughtController";root.transform.SetParent(N("GameplayMenusCanvas").transform,false);var rect=(RectTransform)root.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
 var dialogue=root.GetComponent<DialogueController>();var player=Object.FindAnyObjectByType<PlayerClickMovement>();Set(dialogue,"playerMovement",player);var data=new SerializedObject(dialogue);var panel=(GameObject)data.FindProperty("dialoguePanel").objectReferenceValue;panel.name="CaveThoughtPanel";
 var story=new SerializedObject(Object.FindAnyObjectByType<StoryLocation>());var panels=story.FindProperty("focusPanels");panels.InsertArrayElementAtIndex(panels.arraySize);panels.GetArrayElementAtIndex(panels.arraySize-1).objectReferenceValue=panel;story.ApplyModifiedPropertiesWithoutUndo();
 var pickup=new GameObject("EntrancePickaxe").AddComponent<PickaxePickup>();pickup.transform.position=new Vector3(-6.1f,-1.25f,-1.2f);
 var visual=new GameObject("PickaxeAtEntrance",typeof(SpriteRenderer));visual.transform.SetParent(pickup.transform,false);var renderer=visual.GetComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Tools/Pickaxe-v1.png");renderer.sortingOrder=5;visual.transform.localScale=Vector3.one*(1.15f/renderer.sprite.bounds.size.y);visual.transform.localRotation=Quaternion.Euler(0,0,-18);
 var collider=pickup.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(.1f,.25f,0);collider.size=new Vector3(.85f,1.2f,.25f);
 Set(pickup,"player",player.transform);Set(pickup,"dialogue",dialogue);Set(pickup,"tool",renderer);data=new SerializedObject(pickup);data.FindProperty("interactionDistance").floatValue=.8f;data.FindProperty("actionLabel").stringValue="Pegar picareta";data.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.SaveScene(scene);
 foreach(var name in IslandMapGraphic.Locations){scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");N("JournalTasks").GetComponent<TMP_Text>().fontSize=15;EditorSceneManager.SaveScene(scene);}
 AssetDatabase.SaveAssets();PickaxePickupValidation.Run();
 }
}

