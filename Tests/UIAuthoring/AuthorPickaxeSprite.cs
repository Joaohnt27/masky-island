using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class AuthorPickaxeSprite {
 public static void Run(){
 const string path="Assets/Art/Tools/Pickaxe-v1.png";
 var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Uncompressed;
 var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=new Vector2(.5f,.25f);importer.SetTextureSettings(settings);importer.SaveAndReimport();
 var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
 foreach(var name in IslandMapGraphic.Locations){var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
 var feedback=Object.FindObjectsByType<MiningWorldFeedback>(FindObjectsInactive.Include).First();var so=new SerializedObject(feedback);var tool=(Transform)so.FindProperty("pickaxe").objectReferenceValue;
 foreach(var mesh in tool.GetComponentsInChildren<MeshRenderer>(true))mesh.enabled=false;
 var graphic=new GameObject("PaintedPickaxe",typeof(SpriteRenderer));graphic.transform.SetParent(tool,false);graphic.transform.localScale=Vector3.one*(.95f/sprite.bounds.size.y);var renderer=graphic.GetComponent<SpriteRenderer>();renderer.sprite=sprite;
 var player=Object.FindAnyObjectByType<PlayerClickMovement>();var playerSprite=player.GetComponentInChildren<SpriteRenderer>();renderer.sortingLayerID=playerSprite!=null?playerSprite.sortingLayerID:0;renderer.sortingOrder=playerSprite!=null?playerSprite.sortingOrder+1:5;
 tool.gameObject.SetActive(false);EditorSceneManager.SaveScene(scene);
 }AssetDatabase.SaveAssets();PickaxeSpriteValidation.Run();
 }
}
