using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorQuestJournal {
 static TMP_FontAsset font;
 static GameObject N(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static RectTransform Rect(string n,Transform p,float x,float y,float w,float h){var g=new GameObject(n,typeof(RectTransform));g.layer=5;g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
 static Image Box(string n,Transform p,float x,float y,float w,float h,Color color){var i=Rect(n,p,x,y,w,h).gameObject.AddComponent<Image>();i.color=color;return i;}
 static TMP_Text Text(string n,Transform p,string text,float x,float y,float w,float h,float size){var t=Rect(n,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.color=new Color(.10f,.20f,.19f);t.alignment=TextAlignmentOptions.TopLeft;t.raycastTarget=false;return t;}
 public static void Run(){
 const string path="Assets/Art/UI/Icons/Journal-v1.png";var im=(TextureImporter)AssetImporter.GetAtPath(path);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Single;im.alphaIsTransparency=true;im.mipmapEnabled=false;im.maxTextureSize=256;im.textureCompression=TextureImporterCompression.Uncompressed;im.SaveAndReimport();
 font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
 foreach(string name in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var canvas=N("GameplayMenusCanvas").transform;
 var host=new GameObject("QuestJournalController").AddComponent<QuestJournalUI>();
 var icon=Box("OpenJournal",canvas,0,0,84,84,Color.white);icon.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);icon.preserveAspect=true;icon.rectTransform.anchorMin=icon.rectTransform.anchorMax=icon.rectTransform.pivot=new Vector2(1,0);icon.rectTransform.anchoredPosition=new Vector2(-18,18);var open=icon.gameObject.AddComponent<Button>();open.targetGraphic=icon;icon.gameObject.AddComponent<ButtonSound>();var badge=Object.Instantiate(N("OpenMap").transform.Find("KeyBadge").gameObject,icon.transform,false);badge.name="JournalKeyBadge";badge.GetComponentInChildren<TMP_Text>().text="J";
 var shade=Box("JournalPanel",canvas,0,0,1280,720,new Color(.01f,.025f,.03f,.83f));shade.rectTransform.anchorMin=Vector2.zero;shade.rectTransform.anchorMax=Vector2.one;shade.rectTransform.sizeDelta=Vector2.zero;
 var cover=Box("JournalCover",shade.transform,0,0,1120,640,new Color(.23f,.15f,.09f));
 var left=Box("JournalLeftPage",cover.transform,-270,-15,520,490,new Color(.94f,.87f,.70f));var right=Box("JournalRightPage",cover.transform,270,-15,520,490,new Color(.91f,.83f,.66f));
 Box("JournalSpine",cover.transform,0,-15,12,490,new Color(.45f,.31f,.16f));
 var heading=Text("JournalHeading",cover.transform,"MEU CADERNO",-140,282,760,38,26);heading.color=new Color(.97f,.90f,.74f);
 Text("JournalTasksHeading",left.transform,"O QUE PRECISO FAZER",0,205,440,32,23);
 Text("JournalNotesHeading",right.transform,"MINHAS ANOTAÇÕES",0,205,440,32,23);
 var tasks=Text("JournalTasks",left.transform,"",0,-25,440,400,20);var notes=Text("JournalNotes",right.transform,"",0,-25,440,400,19);
 var closeImage=Box("CloseJournal",cover.transform,440,270,170,40,new Color(.08f,.22f,.23f));var close=closeImage.gameObject.AddComponent<Button>();close.targetGraphic=closeImage;closeImage.gameObject.AddComponent<ButtonSound>();var label=Text("CloseJournalLabel",close.transform,"FECHAR [J / ESC]",0,0,166,30,15);label.alignment=TextAlignmentOptions.Center;label.color=Color.white;
 var footer=Text("JournalFooter",cover.transform,"As tarefas concluídas ficam riscadas. Novas anotações surgem conforme você avança.",0,-292,1020,26,16);footer.color=new Color(.97f,.90f,.74f);footer.alignment=TextAlignmentOptions.Center;
 Set(host,"panel",shade.gameObject);Set(host,"openButton",open);Set(host,"player",Object.FindAnyObjectByType<PlayerClickMovement>());var story=Object.FindAnyObjectByType<StoryLocation>();Set(host,"story",story);Set(host,"heading",heading);Set(host,"missions",tasks);Set(host,"notes",notes);
 UnityEventTools.AddPersistentListener(open.onClick,host.Toggle);UnityEventTools.AddPersistentListener(close.onClick,host.Close);
 var so=new SerializedObject(story);var panels=so.FindProperty("focusPanels");panels.InsertArrayElementAtIndex(panels.arraySize);panels.GetArrayElementAtIndex(panels.arraySize-1).objectReferenceValue=shade.gameObject;so.ApplyModifiedPropertiesWithoutUndo();shade.gameObject.SetActive(false);icon.gameObject.SetActive(false);
 EditorSceneManager.SaveScene(scene);}
 AssetDatabase.SaveAssets();Debug.Log("JOURNAL AUTHORED");EditorApplication.Exit(0);
 }
}
