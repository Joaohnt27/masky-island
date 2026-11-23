using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorDiaryPickup {
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Move(string n,float x,float y,float w,float h){var r=(RectTransform)N(n).transform;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
 public static void Run(){
 const string path="Assets/Art/UI/Journal/OpenJournal-v1.png";var import=(TextureImporter)AssetImporter.GetAtPath(path);import.textureType=TextureImporterType.Sprite;import.spriteImportMode=SpriteImportMode.Single;import.alphaIsTransparency=true;import.mipmapEnabled=false;import.maxTextureSize=2048;import.SaveAndReimport();var art=AssetDatabase.LoadAssetAtPath<Sprite>(path);
 foreach(var name in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var cover=N("JournalCover");
 foreach(var image in cover.GetComponentsInChildren<Image>(true))if(image.gameObject!=cover&&image.name!="CloseJournal")image.enabled=false;
 foreach(var edge in cover.GetComponentsInChildren<Outline>(true))if(edge.name!="CloseJournal")edge.enabled=false;
 cover.GetComponent<Image>().sprite=art;cover.GetComponent<Image>().color=Color.white;Move("JournalCover",0,-10,1140,680);
 Move("JournalHeading",-140,315,760,42);Move("CloseJournal",440,315,170,40);
 Move("JournalLeftPage",-260,-10,490,470);Move("JournalRightPage",260,-10,490,470);
 Move("JournalTasksHeading",0,200,410,34);Move("JournalNotesHeading",0,200,410,34);
 Move("JournalTasks",0,-20,410,370);Move("JournalNotes",0,-20,410,370);
 N("JournalTasks").GetComponent<TMP_Text>().fontSize=17;N("JournalNotes").GetComponent<TMP_Text>().fontSize=16;
 Move("JournalFooter",0,-321,900,24);
 var journal=Object.FindAnyObjectByType<QuestJournalUI>();var data=new SerializedObject(journal);var headings=data.FindProperty("writtenHeadings");headings.arraySize=3;string[] names={"JournalTasksHeading","JournalNotesHeading","JournalFooter"};for(int i=0;i<3;i++)headings.GetArrayElementAtIndex(i).objectReferenceValue=N(names[i]);data.ApplyModifiedPropertiesWithoutUndo();
 if(name=="Beach"){
 var player=Object.FindAnyObjectByType<PlayerClickMovement>();var go=new GameObject("BeachDiary");go.transform.position=new Vector3(-.8f,-2.8f,-1.1f);
 var visual=new GameObject("DiaryOnSand",typeof(SpriteRenderer));visual.transform.SetParent(go.transform,false);var sprite=visual.GetComponent<SpriteRenderer>();sprite.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icons/Journal-v1.png");sprite.sortingOrder=4;visual.transform.localScale=Vector3.one*(.75f/sprite.sprite.bounds.size.y);visual.transform.localRotation=Quaternion.Euler(0,0,-18);
 var collider=go.AddComponent<BoxCollider>();collider.size=new Vector3(.7f,.7f,.25f);
 var pickup=go.AddComponent<JournalPickup>();Set(pickup,"player",player.transform);Set(pickup,"book",sprite);Set(pickup,"dialogue",Object.FindAnyObjectByType<DialogueController>());
 var serialized=new SerializedObject(pickup);serialized.FindProperty("interactionDistance").floatValue=.8f;serialized.FindProperty("actionLabel").stringValue="Pegar diário";serialized.ApplyModifiedPropertiesWithoutUndo();
 }
 EditorSceneManager.SaveScene(scene);
 }AssetDatabase.SaveAssets();DiaryPickupValidation.Run();
 }
}
