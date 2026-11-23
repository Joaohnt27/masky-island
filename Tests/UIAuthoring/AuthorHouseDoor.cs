using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AuthorHouseDoor
{
    const string Sheet = "Assets/Art/Doors/HouseDoor-v1.png";
    static void Ref(Object owner, string name, Object value)
    {
        var data = new SerializedObject(owner); data.FindProperty(name).objectReferenceValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }
    public static Sprite[] ImportSheet(string sheet, string prefix = "HouseDoor_")
    {
        AssetDatabase.Refresh();
        var texture = new Texture2D(2, 2); texture.LoadImage(File.ReadAllBytes(sheet));
        var pixels = texture.GetPixels32(); int cw = texture.width / 5, ch = texture.height;
        var bounds = new RectInt[5];
        for (int i = 0; i < 5; i++)
        {
            int x0 = cw, y0 = ch, x1 = -1, y1 = -1;
            for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++)
                if (pixels[y * texture.width + i * cw + x].a > 32)
                { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
            if (x1 < x0) throw new Exception("Quadro vazio na porta");
            bounds[i] = new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }
        int width = bounds.Max(b => b.width), height = bounds.Max(b => b.height);
        var metadata = new SpriteMetaData[5];
        for (int i = 0; i < 5; i++)
        {
            var b = bounds[i]; int left = Mathf.Clamp(b.x + (b.width - width) / 2, 0, cw - width);
            metadata[i] = new SpriteMetaData { name = prefix + i,
                rect = new Rect(i * cw + left, b.y, width, height), alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(.5f, .5f) };
        }
        Object.DestroyImmediate(texture);
        var importer = (TextureImporter)AssetImporter.GetAtPath(sheet);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
#pragma warning disable 0618
        importer.spritesheet = metadata;
#pragma warning restore 0618
        importer.SaveAndReimport();
        var frames = AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (frames.Length != 5) throw new Exception("Porta precisa de cinco quadros");
        return frames;
    }
    public static void Run()
    {
        var frames = ImportSheet(Sheet);
        Sound("DoorOpen", .5f, false); Sound("DoorClose", .4f, true); AssetDatabase.Refresh();
        foreach (string name in new[] { "Beach", "Island" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            var door = Object.FindObjectsByType<DoorInteraction>(FindObjectsInactive.Exclude).First(d => d.SceneName == "House");
            var bg = GameObject.Find("StoryBackground").GetComponent<SpriteRenderer>();
            // Coordenadas da moldura nos backgrounds originais de 1672 x 941.
            Rect area = name == "Beach" ? new Rect(1353, 168, 66, 90) : new Rect(209, 209, 78, 121);
            Vector3 point = new Vector3(bg.bounds.min.x + area.center.x / 1672f * bg.bounds.size.x,
                bg.bounds.max.y - area.center.y / 941f * bg.bounds.size.y, -1.05f);
            var child = door.transform.Find("AnimatedHousePortal");
            if (child == null) { child = new GameObject("AnimatedHousePortal", typeof(SpriteRenderer)).transform; child.SetParent(door.transform); }
            var renderer = child.GetComponent<SpriteRenderer>(); renderer.sprite = frames[0];
            renderer.sharedMaterial = bg.sharedMaterial; renderer.sortingOrder = -90;
            child.position = point;
            child.localScale = new Vector3(area.width / 1672f * bg.bounds.size.x / frames[0].bounds.size.x,
                area.height / 941f * bg.bounds.size.y / frames[0].bounds.size.y, 1);
            var collider = door.GetComponent<BoxCollider>();
            collider.center = door.transform.InverseTransformPoint(point);
            collider.size = new Vector3(area.width / 1672f * bg.bounds.size.x + .18f, area.height / 941f * bg.bounds.size.y + .18f, .4f);
            var animation = door.GetComponent<AnimatedDoor>();
            if (animation == null) animation = door.gameObject.AddComponent<AnimatedDoor>();
            Ref(animation, "player", Object.FindAnyObjectByType<PlayerClickMovement>()); Ref(animation, "portal", renderer);
            var data = new SerializedObject(animation); var array = data.FindProperty("frames"); array.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            data.ApplyModifiedPropertiesWithoutUndo(); Ref(door, "animatedDoor", animation);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets(); HouseDoorValidation.Run();
    }

    // Sons originais de dobradiça e batente, no mesmo pipeline dos efeitos cartunescos existentes.
    static void Sound(string name, float duration, bool close)
    {
        const int rate = 22050; int count = (int)(rate * duration); var random = new System.Random(close ? 72 : 71);
        string path = "Assets/Resources/CartoonSfx/" + name + ".wav";
        using (var file = new BinaryWriter(File.Create(path)))
        {
            file.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); file.Write(36 + count * 2);
            file.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); file.Write(16); file.Write((short)1);
            file.Write((short)1); file.Write(rate); file.Write(rate * 2); file.Write((short)2); file.Write((short)16);
            file.Write(System.Text.Encoding.ASCII.GetBytes("data")); file.Write(count * 2);
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)rate, u = t / duration;
                double phase = 2 * Math.PI * ((close ? 310 : 190) * t + (close ? -150 : 160) * t * t);
                double grain = .7 + .3 * Math.Sin(2 * Math.PI * 27 * t);
                double value = (.5 * Math.Sin(phase) + .18 * Math.Sin(phase * 3) + .06 * (random.NextDouble() * 2 - 1)) * grain * Math.Sin(Math.PI * u);
                double knockTime = t - duration * .78;
                if (close && knockTime > 0) value += .35 * Math.Sin(2 * Math.PI * 120 * knockTime) * Math.Exp(-knockTime * 48);
                file.Write((short)(Math.Clamp(value * .34, -1, 1) * 32767));
            }
        }
    }
}
