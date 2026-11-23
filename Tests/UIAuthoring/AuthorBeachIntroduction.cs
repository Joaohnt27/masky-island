using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorBeachIntroduction {
 static TMP_FontAsset font;
 static GameObject Named(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static RectTransform Rect(string n,Transform p,float x,float y,float w,float h){var g=new GameObject(n,typeof(RectTransform));g.layer=5;g.transform.SetParent(p,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
 static TMP_Text Text(string n,Transform p,string v,float x,float y,float w,float h,float size){var t=Rect(n,p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=v;t.fontSize=size;t.color=new Color(.11f,.22f,.23f);t.alignment=TextAlignmentOptions.MidlineLeft;t.raycastTarget=false;return t;}
 public static void Run(){
 const string path="Assets/Art/UI/CoastalDialogue-v1.png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.SaveAndReimport();
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Beach.unity");font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
 var canvas=Named("GameplayMenusCanvas").transform;var player=Object.FindAnyObjectByType<PlayerClickMovement>();var story=Object.FindAnyObjectByType<StoryLocation>();
 var panel=Rect("BeachIntroductionPanel",canvas,0,0,1280,720);panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;panel.sizeDelta=Vector2.zero;var shade=panel.gameObject.AddComponent<Image>();shade.color=new Color(.025f,.05f,.06f,.12f);
 var card=Rect("CoastalDialogueCard",panel,0,190,1080,300);card.anchorMin=card.anchorMax=new Vector2(.5f,0);var art=card.gameObject.AddComponent<RawImage>();art.texture=AssetDatabase.LoadAssetAtPath<Texture>(path);art.raycastTarget=false;
 var portrait=Rect("PlayerPortrait",card,-380,5,105,175).gameObject.AddComponent<Image>();portrait.sprite=player.GetComponentInChildren<SpriteRenderer>().sprite;portrait.preserveAspect=true;portrait.raycastTarget=false;
 var speaker=Text("IntroductionSpeaker",card,"VOCÊ",-15,85,570,28,20);speaker.fontStyle=FontStyles.Bold;
 var body=Text("IntroductionText",card,"",65,12,730,110,28);
 var button=Rect("IntroductionContinue",card,230,-85,290,44).gameObject.AddComponent<Image>();button.color=new Color(.12f,.25f,.26f);var next=button.gameObject.AddComponent<Button>();next.targetGraphic=button;button.gameObject.AddComponent<ButtonSound>();var label=Text("ContinueLabel",button.transform,"CONTINUAR  ›",0,0,270,40,18);label.alignment=TextAlignmentOptions.Center;label.color=new Color(.97f,.91f,.75f);
 var controller=new GameObject("BeachIntroductionController").AddComponent<DialogueController>();Set(controller,"dialoguePanel",panel.gameObject);Set(controller,"dialogueText",body);Set(controller,"npcNameText",speaker);Set(controller,"playerMovement",player);Set(controller,"advanceLabel",label);
 var ds=new SerializedObject(controller);ds.FindProperty("playerIntroduction").boolValue=true;ds.FindProperty("npcName").stringValue="VOCÊ";ds.ApplyModifiedPropertiesWithoutUndo();UnityEventTools.AddPersistentListener(next.onClick,controller.NextLine);
 var ss=new SerializedObject(story);ss.FindProperty("hideObjectiveHeader").boolValue=true;var focus=ss.FindProperty("focusPanels");focus.InsertArrayElementAtIndex(focus.arraySize);focus.GetArrayElementAtIndex(focus.arraySize-1).objectReferenceValue=panel.gameObject;ss.ApplyModifiedPropertiesWithoutUndo();Named("StoryObjectiveCard").SetActive(false);panel.gameObject.SetActive(false);
 EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("BEACH INTRO AUTHORED");EditorApplication.Exit(0);
 }
}
