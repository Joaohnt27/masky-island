using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorHudIcons {
 static GameObject Named(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 static void Set(Object o,string f,Object v){var s=new SerializedObject(o);s.FindProperty(f).objectReferenceValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
 static void Style(Button button,Sprite sprite,float y,string key){
 var r=(RectTransform)button.transform;r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(18,y);r.sizeDelta=new Vector2(84,84);
 foreach(Transform child in button.transform.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
 var image=button.GetComponent<Image>();image.sprite=sprite;image.type=Image.Type.Simple;image.preserveAspect=true;image.color=Color.white;button.targetGraphic=image;
 var badge=new GameObject("KeyBadge",typeof(RectTransform),typeof(Image));badge.transform.SetParent(button.transform,false);var b=(RectTransform)badge.transform;b.anchorMin=b.anchorMax=b.pivot=Vector2.one;b.anchoredPosition=new Vector2(0,-60);b.sizeDelta=new Vector2(24,24);badge.GetComponent<Image>().color=new Color(.04f,.13f,.14f,.95f);badge.GetComponent<Image>().raycastTarget=false;
 var label=new GameObject("Key",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(badge.transform,false);var lr=(RectTransform)label.transform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=lr.offsetMax=Vector2.zero;var t=label.GetComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");t.fontSize=16;t.alignment=TextAlignmentOptions.Center;t.color=new Color(.98f,.91f,.73f);t.text=key;t.raycastTarget=false;
 }
 public static void Run(){
 foreach(string name in new[]{"Map","Inventory"}){var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Art/UI/Icons/"+name+"-v1.png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=256;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
 var mapSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icons/Map-v1.png");var bagSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Icons/Inventory-v1.png");
 foreach(var name in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var map=Named("OpenMap").GetComponent<Button>();Style(map,mapSprite,18,"M");
 var go=new GameObject("OpenInventory",typeof(RectTransform),typeof(Image),typeof(Button),typeof(ButtonSound));go.layer=5;go.transform.SetParent(map.transform.parent,false);go.transform.SetSiblingIndex(map.transform.GetSiblingIndex()+1);var button=go.GetComponent<Button>();Style(button,bagSprite,114,"I");
 var inventory=Object.FindAnyObjectByType<CrystalInventoryUI>();Set(inventory,"openButton",button);Set(inventory,"story",Object.FindAnyObjectByType<StoryLocation>());UnityEventTools.AddPersistentListener(button.onClick,inventory.Toggle);go.SetActive(false);
 if(name=="Cave"){var progress=(RectTransform)Named("WorldMiningPanel").transform;progress.anchorMin=progress.anchorMax=new Vector2(.5f,0);progress.pivot=new Vector2(.5f,0);progress.anchoredPosition=new Vector2(0,progress.anchoredPosition.y);}
 EditorSceneManager.SaveScene(scene);}
 AssetDatabase.SaveAssets();Debug.Log("HUD ICONS AUTHORED");EditorApplication.Exit(0);
 }
}
