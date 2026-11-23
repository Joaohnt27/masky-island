using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AuthorNpcIdle
{
    const string Sheet = "Assets/Art/Characters/NPC/NpcIdle-v1.png";
    const string Folder = "Assets/Animations/NPC";

    public static void Run()
    {
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(Sheet));
        var pixels = texture.GetPixels32();
        int width = texture.width / 4, height = texture.height;
        var bounds = new RectInt[4];
        for (int frame = 0; frame < 4; frame++)
        {
            int minX = width, minY = height, maxX = -1, maxY = -1;
            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                if (pixels[y * texture.width + frame * width + x].a > 32)
                { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
            if (maxX < minX) throw new Exception("Quadro vazio no idle do NPC");
            bounds[frame] = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
        float bodyHeight = bounds.Select(b => b.height).OrderBy(h => h).ElementAt(1);
        var sprites = new SpriteMetaData[4];
        for (int i = 0; i < 4; i++) sprites[i] = new SpriteMetaData {
            name = "NpcIdle_" + i, rect = new Rect(i * width, 0, width, height),
            alignment = (int)SpriteAlignment.Custom,
            pivot = new Vector2((bounds[i].x + bounds[i].width * .5f) / width, (bounds[i].y + bodyHeight * .5f) / height)
        };
        UnityEngine.Object.DestroyImmediate(texture);
        var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = bodyHeight / 1.35f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
#pragma warning disable 0618
        importer.spritesheet = sprites;
#pragma warning restore 0618
        importer.SaveAndReimport();
        var frames = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        if (frames.Length != 4) throw new Exception("Idle do NPC precisa de quatro quadros");
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder + "/NpcIdle.anim");
        if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, Folder + "/NpcIdle.anim"); }
        clip.frameRate = 2;
        int[] order = { 0, 1, 2, 3, 2, 1, 0 };
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding {
            path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite"
        }, order.Select((frame, i) => new ObjectReferenceKeyframe { time = i * .5f, value = frames[frame] }).ToArray());
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Folder + "/NPC.controller");
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(Folder + "/NPC.controller");
        var machine = controller.layers[0].stateMachine;
        foreach (var child in machine.states) machine.RemoveState(child.state);
        var state = machine.AddState("Idle"); state.motion = clip; machine.defaultState = state;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        var npc = UnityEngine.Object.FindAnyObjectByType<NPCInteraction>().GetComponentInParent<SpriteRenderer>();
        if (npc == null) throw new Exception("SpriteRenderer do NPC ausente");
        var animator = npc.GetComponent<Animator>();
        if (animator == null) animator = npc.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        npc.sprite = frames[0];
        EditorUtility.SetDirty(clip); EditorUtility.SetDirty(controller);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        NpcIdleValidation.Run();
    }
}
