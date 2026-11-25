using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object=UnityEngine.Object;
public static class AuthorLantern {
 static void Ref(SerializedObject s,string key,Object val)=>s.FindProperty(key).objectReferenceValue=val;
 public static void Run(){
 const string path="Assets/Art/Items/Lantern/Lantern-v1.png";
 var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.textureShape=TextureImporterShape.Texture2D;imp.spriteImportMode=SpriteImportMode.Single;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();
 var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
 foreach(string name in new[]{"Beach","House","Island","Cave","LighthouseExterior","Lighthouse"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
 foreach(var old in Object.FindObjectsByType<PlayerLantern>(FindObjectsInactive.Include))Object.DestroyImmediate(old.gameObject);
 foreach(var old in Object.FindObjectsByType<LanternPickup>(FindObjectsInactive.Include))Object.DestroyImmediate(old.gameObject);
 var player=Object.FindAnyObjectByType<PlayerClickMovement>();
 var existing=player.transform.Find("CarriedLantern");if(existing!=null)Object.DestroyImmediate(existing.gameObject);
 var controller=new GameObject("PlayerLanternController").AddComponent<PlayerLantern>();
 var carried=new GameObject("CarriedLantern").AddComponent<SpriteRenderer>();carried.transform.SetParent(player.transform,false);carried.transform.localPosition=new Vector3(.44f,-.16f,-.15f);carried.transform.localScale=Vector3.one*(.58f/sprite.bounds.size.y);carried.sprite=sprite;carried.sortingOrder=30;
 var ui=new GameObject("LanternHint",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(CanvasGroup));ui.transform.SetParent(controller.transform,false);var canvas=ui.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1;var scaler=ui.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);var group=ui.GetComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
 var text=new GameObject("LanternKey",typeof(RectTransform)).AddComponent<TextMeshProUGUI>();text.transform.SetParent(ui.transform,false);text.rectTransform.anchorMin=text.rectTransform.anchorMax=new Vector2(.5f,0);text.rectTransform.anchoredPosition=new Vector2(0,32);text.rectTransform.sizeDelta=new Vector2(450,45);text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");text.fontSize=20;text.color=new Color(1,.91f,.65f);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
 var data=new SerializedObject(controller);Ref(data,"player",player);Ref(data,"carried",carried);Ref(data,"hint",text);Ref(data,"hintGroup",group);data.ApplyModifiedPropertiesWithoutUndo();
 foreach(var chase in Object.FindObjectsByType<StormPursuit>(FindObjectsInactive.Include)){
 foreach(var line in chase.GetComponentsInChildren<LineRenderer>(true))Object.DestroyImmediate(line.gameObject);
 var s=new SerializedObject(chase);s.FindProperty("firstDelay").floatValue=6;s.ApplyModifiedPropertiesWithoutUndo();}
 foreach(var bag in Object.FindObjectsByType<CrystalInventoryUI>(FindObjectsInactive.Include)){var s=new SerializedObject(bag);var a=s.FindProperty("itemVisuals");int index=-1;for(int i=0;i<a.arraySize;i++)if(a.GetArrayElementAtIndex(i).FindPropertyRelative("item").intValue==(int)GameManager.ItemId.Lantern)index=i;if(index<0){index=a.arraySize;a.arraySize++;}var entry=a.GetArrayElementAtIndex(index);entry.FindPropertyRelative("item").intValue=(int)GameManager.ItemId.Lantern;entry.FindPropertyRelative("texture").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Texture2D>(path);entry.FindPropertyRelative("uv").rectValue=new Rect(0,0,1,1);s.ApplyModifiedPropertiesWithoutUndo();}
 if(name=="House"){
 var go=new GameObject("HouseLantern",typeof(BoxCollider));go.transform.position=new Vector3(5.2f,-1.1f,-1.15f);go.GetComponent<BoxCollider>().size=new Vector3(.65f,.9f,.3f);var visual=go.AddComponent<SpriteRenderer>();visual.sprite=sprite;visual.sortingOrder=15;go.transform.localScale=Vector3.one*(.85f/sprite.bounds.size.y);go.GetComponent<BoxCollider>().size=new Vector3(sprite.bounds.size.x,sprite.bounds.size.y,2);
 var pickup=go.AddComponent<LanternPickup>();var point=new GameObject("LanternApproach");point.transform.SetParent(go.transform);point.transform.position=new Vector3(5.2f,-1.7f,-1);
 var s=new SerializedObject(pickup);Ref(s,"player",player.transform);Ref(s,"visual",visual);Ref(s,"dialogue",Object.FindAnyObjectByType<DialogueController>());Ref(s,"approachPoint",point.transform);s.FindProperty("interactionDistance").floatValue=.8f;s.FindProperty("actionLabel").stringValue="Pegar lampião";s.ApplyModifiedPropertiesWithoutUndo();
 foreach(var puzzle in Object.FindObjectsByType<SymbolSequencePuzzle>(FindObjectsInactive.Include)){var p=new SerializedObject(puzzle);if(p.FindProperty("memory").enumValueIndex==0){p.FindProperty("nextStepText").stringValue="Deixei um lampião junto à lareira.\nVou pegá-lo e usar E para afastar a Sombra.";p.ApplyModifiedPropertiesWithoutUndo();}}
 }
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();System.Type.GetType("StormPursuitValidation, Assembly-CSharp").GetMethod("Run").Invoke(null,null);
 }
}
