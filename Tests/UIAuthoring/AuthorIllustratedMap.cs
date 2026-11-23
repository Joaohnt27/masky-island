using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
public static class AuthorIllustratedMap {
 static GameObject Named(string n)=>Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.name==n);
 public static void Run(){
 const string path="Assets/Art/Maps/IslandMap-v1.png";
 var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.SaveAndReimport();
 var texture=AssetDatabase.LoadAssetAtPath<Texture>(path);
 Vector2[] positions={new Vector2(.22f,.87f),new Vector2(.21f,.57f),new Vector2(.49f,.59f),new Vector2(.80f,.57f),new Vector2(.65f,.29f)};
 foreach(var name in IslandMapGraphic.Locations){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
 var ocean=Named("MapOcean");Object.DestroyImmediate(Named("IslandDrawing"));Object.DestroyImmediate(Named("Compass"));
 var go=new GameObject("IllustratedIslandMap",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(ocean.transform,false);go.transform.SetAsFirstSibling();var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;var raw=go.GetComponent<RawImage>();raw.texture=texture;raw.raycastTarget=false;
 for(int i=0;i<5;i++){var pin=Named("MapPin"+IslandMapGraphic.Locations[i]);var r=(RectTransform)pin.transform;r.anchoredPosition=new Vector2((positions[i].x-.5f)*700,(.5f-positions[i].y)*380);r.sizeDelta=new Vector2(122,40);var t=pin.GetComponentInChildren<TMP_Text>();t.rectTransform.sizeDelta=new Vector2(118,38);t.fontSize=18;}
 var map=Object.FindAnyObjectByType<IslandMapUI>();var so=new SerializedObject(map);so.FindProperty("controls").objectReferenceValue=Named("StoryControls").GetComponent<TMP_Text>();so.ApplyModifiedPropertiesWithoutUndo();Named("OpenMap").SetActive(false);
 Named("StoryControls").GetComponent<TMP_Text>().text="W A S D  mover    •    Clique para interagir    •    I  inventário    •    Esc  pausa";
 EditorSceneManager.SaveScene(scene);
 }
 AssetDatabase.SaveAssets();Debug.Log("ILLUSTRATED MAP AUTHORED");EditorApplication.Exit(0);
 }
}
