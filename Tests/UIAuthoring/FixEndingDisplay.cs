using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class FixEndingDisplay
{
 public static void Run()
 {
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/Ending.unity");
  var root=GameObject.Find("EndingDisplayCamera");
  if(root==null)root=new GameObject("EndingDisplayCamera",typeof(Camera));
  var camera=root.GetComponent<Camera>();root.tag="MainCamera";root.transform.position=new Vector3(0,0,-10);
  camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.cullingMask=0;
  camera.targetDisplay=0;camera.depth=-100;camera.orthographic=true;camera.orthographicSize=5;
  camera.allowHDR=false;camera.allowMSAA=false;
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();EndingDisplayValidation.Run();
 }
}
