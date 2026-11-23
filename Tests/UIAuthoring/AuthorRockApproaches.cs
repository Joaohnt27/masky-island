using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class AuthorRockApproaches {
 public static void Run(){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/Cave.unity");
 var spot=Object.FindAnyObjectByType<MiningSpot>();
 var rocks=spot.GetComponentsInChildren<BoxCollider>().Where(c=>c.name=="MineralVein").OrderBy(c=>c.transform.position.x).ToArray();
 if(rocks.Length!=3)throw new System.Exception("Expected exactly three rock colliders");
 var points=new[]{new Vector3(-3.7f,1.05f,-1),new Vector3(4.0f,1.15f,-1),new Vector3(6.8f,-.05f,-1)};
 var s=new SerializedObject(spot);var array=s.FindProperty("rockTargets");array.arraySize=3;
 for(int i=0;i<3;i++){
 var go=new GameObject("RockApproach"+(i+1));go.transform.SetParent(spot.transform);go.transform.position=points[i];
 array.GetArrayElementAtIndex(i).FindPropertyRelative("collider").objectReferenceValue=rocks[i];
 array.GetArrayElementAtIndex(i).FindPropertyRelative("approach").objectReferenceValue=go.transform;
 if(i==0)s.FindProperty("approachPoint").objectReferenceValue=go.transform;
 }
 var socket=new GameObject("SocketApproach");socket.transform.SetParent(spot.transform);socket.transform.position=new Vector3(0,1,-1);s.FindProperty("socketApproach").objectReferenceValue=socket.transform;s.FindProperty("miningReach").floatValue=.25f;s.ApplyModifiedPropertiesWithoutUndo();
 var player=Object.FindAnyObjectByType<PlayerClickMovement>();var ps=new SerializedObject(player);ps.FindProperty("walkMax").vector2Value=new Vector2(7.4f,1.2f);ps.ApplyModifiedPropertiesWithoutUndo();
 EditorSceneManager.SaveScene(scene);Debug.Log("ROCK APPROACHES AUTHORED");EditorApplication.Exit(0);
 }
}
