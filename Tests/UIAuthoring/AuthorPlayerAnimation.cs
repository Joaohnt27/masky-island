using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

// Rodar na cópia de autoria e copiar apenas os assets e as cinco cenas validados.
public static class AuthorPlayerAnimation
{
    const string Art = "Assets/Art/Characters/Player";
    const string Anim = "Assets/Animations/Player";
    static readonly string[] Directions = { "Front", "Back", "Left", "Right" };
    static readonly string[] Scenes = { "Beach", "House", "Island", "Cave", "Lighthouse" };

    public static void Run()
    {
        Directory.CreateDirectory(Anim);
        AssetDatabase.Refresh();
        Slice(Art + "/PlayerWalk-v1.png", 6, 4, false);
        Slice(Art + "/PlayerIdle-v1.png", 4, 2, true);
        var walks = AssetDatabase.LoadAllAssetsAtPath(Art + "/PlayerWalk-v1.png").OfType<Sprite>().ToArray();
        var idles = AssetDatabase.LoadAllAssetsAtPath(Art + "/PlayerIdle-v1.png").OfType<Sprite>().ToArray();
        string controllerPath = Anim + "/Player.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var machine = controller.layers[0].stateMachine;
        foreach (var child in machine.states)
            if (child.state.name.StartsWith("Idle") || child.state.name.StartsWith("Walk"))
                machine.RemoveState(child.state);
        for (int d = 0; d < 4; d++)
        {
            foreach (bool walking in new[] { false, true })
            {
                string stateName = (walking ? "Walk" : "Idle") + Directions[d];
                Sprite[] frames = (walking ? walks : idles).Where(s => s.name.StartsWith(stateName + "_"))
                    .OrderBy(s => s.name).ToArray();
                if (frames.Length != (walking ? 6 : 2)) throw new Exception("Frames ausentes: " + stateName);
                string path = Anim + "/" + stateName + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
                clip.frameRate = walking ? 8 : 2;
                var keys = new ObjectReferenceKeyframe[frames.Length + 1];
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = frames[i % frames.Length] };
                AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding {
                    path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite"
                }, keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                var state = machine.AddState(stateName, new Vector3(d * 230, walking ? 160 : 20));
                state.motion = clip;
                if (stateName == "IdleFront") machine.defaultState = state;
                EditorUtility.SetDirty(clip);
            }
        }
        EditorUtility.SetDirty(controller);
        foreach (string name in Scenes)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            var player = UnityEngine.Object.FindAnyObjectByType<PlayerClickMovement>();
            if (player == null) throw new Exception("Player ausente: " + name);
            var animator = player.GetComponent<Animator>();
            if (animator == null) animator = player.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (player.GetComponent<PlayerSpriteAnimation>() == null) player.gameObject.AddComponent<PlayerSpriteAnimation>();
            player.GetComponent<SpriteRenderer>().sprite = idles.First(s => s.name == "IdleFront_0");
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        PlayerAnimationValidation.Run();
    }

    static void Slice(string path, int columns, int rows, bool idle)
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));
        var pixels = texture.GetPixels32();
        int cw = texture.width / columns, ch = texture.height / rows;
        var bounds = new RectInt[columns * rows];
        for (int row = 0; row < rows; row++) for (int col = 0; col < columns; col++)
        {
            int left = col * cw, bottom = texture.height - (row + 1) * ch;
            int minX = cw, minY = ch, maxX = -1, maxY = -1;
            for (int y = 0; y < ch; y++) for (int x = 0; x < cw; x++)
                if (pixels[(bottom + y) * texture.width + left + x].a > 32)
                { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
            if (maxX < minX || maxY < minY) throw new Exception("Célula vazia: " + path);
            bounds[row * columns + col] = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
        // Mesmo tamanho aparente e pés na mesma altura, apesar das margens da arte gerada.
        float height = bounds.Select(b => b.height).OrderBy(h => h).ElementAt(bounds.Length / 2);
        var metadata = new SpriteMetaData[bounds.Length];
        for (int row = 0; row < rows; row++) for (int col = 0; col < columns; col++)
        {
            int i = row * columns + col;
            var b = bounds[i];
            metadata[i] = new SpriteMetaData {
                name = (idle ? "Idle" : "Walk") + Directions[idle ? col : row] + "_" + (idle ? row : col),
                rect = new Rect(col * cw, texture.height - (row + 1) * ch, cw, ch),
                alignment = (int)SpriteAlignment.Custom,
                pivot = new Vector2((b.x + b.width * .5f) / cw, (b.y + height * .5f) / ch)
            };
        }
        UnityEngine.Object.DestroyImmediate(texture);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = height / 1.35f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
#pragma warning disable 0618
        importer.spritesheet = metadata;
#pragma warning restore 0618
        importer.SaveAndReimport();
    }
}
