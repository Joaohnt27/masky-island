using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AuthorHouseExit
{
    static void Ref(Object owner, string name, Object value)
    {
        var data = new SerializedObject(owner); data.FindProperty(name).objectReferenceValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    public static void Run()
    {
        var frames = AuthorHouseDoor.ImportSheet("Assets/Art/Doors/HouseExit-v1.png", "HouseExit_");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/House.unity");
        var door = Object.FindObjectsByType<DoorInteraction>(FindObjectsInactive.Exclude).First(d => d.SceneName == "Island");
        var bg = GameObject.Find("StoryBackground").GetComponent<SpriteRenderer>();
        // Cobre o vão e a folha original, preservando a lanterna e o barril ao redor.
        Rect area = new Rect(12, 269, 262, 423);
        Vector3 point = new Vector3(bg.bounds.min.x + area.center.x / 1672f * bg.bounds.size.x,
            bg.bounds.max.y - area.center.y / 941f * bg.bounds.size.y, -1.05f);
        var child = door.transform.Find("AnimatedHouseExit");
        if (child == null) { child = new GameObject("AnimatedHouseExit", typeof(SpriteRenderer)).transform; child.SetParent(door.transform); }
        var renderer = child.GetComponent<SpriteRenderer>(); renderer.sprite = frames[0];
        renderer.sharedMaterial = bg.sharedMaterial; renderer.sortingOrder = -90;
        child.position = point;
        // Plano da parede lateral: estreita a porta e faz o lado direito recuar,
        // mantendo os batentes verticais e o mesmo encaixe em todos os quadros.
        child.localRotation = Quaternion.AngleAxis(12f, Vector3.right) * Quaternion.AngleAxis(30f, Vector3.up);
        child.localScale = new Vector3(area.width / 1672f * bg.bounds.size.x / frames[0].bounds.size.x,
            area.height / 941f * bg.bounds.size.y / frames[0].bounds.size.y, 1);
        var collider = door.GetComponent<BoxCollider>();
        collider.center = door.transform.InverseTransformPoint(point);
        collider.size = new Vector3(renderer.bounds.size.x + .1f, renderer.bounds.size.y + .1f, .4f);
        var label = door.GetComponentInChildren<TMPro.TextMeshPro>();
        if (label != null)
        {
            label.text = "‹  Centro";
            label.transform.position = new Vector3(-5.5f, -2.3f, -1.15f);
            label.rectTransform.sizeDelta = new Vector2(2.3f, .5f);
            var labelCollider = label.GetComponent<BoxCollider>();
            if (labelCollider != null) labelCollider.size = new Vector3(2.3f, .6f, .3f);
        }
        var animation = door.GetComponent<AnimatedDoor>();
        if (animation == null) animation = door.gameObject.AddComponent<AnimatedDoor>();
        Ref(animation, "player", Object.FindAnyObjectByType<PlayerClickMovement>()); Ref(animation, "portal", renderer);
        var data = new SerializedObject(animation); var array = data.FindProperty("frames"); array.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        data.ApplyModifiedPropertiesWithoutUndo(); Ref(door, "animatedDoor", animation);
        EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); HouseExitValidation.Run();
    }
}
