using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

public static class AuthorStormShadow
{
    private static void Reference(SerializedObject data, string name, Object value)
        => data.FindProperty(name).objectReferenceValue = value;

    [MenuItem("Masky Island/Install Storm Shadow")]
    public static void Run()
    {
        const string path = "Assets/Art/Characters/StormShadow/StormShadow-v1.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePivot = Vector2.one * .5f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 1024;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new System.Exception("Sprite da sombra não foi importado como textura 2D.");
        foreach (string name in new[] { "Beach", "Cave", "LighthouseExterior", "Lighthouse" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            foreach (var old in Object.FindObjectsByType<StormShadow>(FindObjectsInactive.Include))
                Object.DestroyImmediate(old.gameObject);
            foreach (var old in Object.FindObjectsByType<StormPursuit>(FindObjectsInactive.Include))
                Object.DestroyImmediate(old.gameObject);
            if (name != "Lighthouse") BuildPursuit(sprite);
            if (name == "Cave" || name == "LighthouseExterior")
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                continue;
            }
            var host = new GameObject("StormShadowPresentation", typeof(RectTransform));
            var controller = host.AddComponent<StormShadow>();
            Transform parent;
            bool finale = name == "Lighthouse";
            if (finale)
            {
                // Dentro apenas da ilustração: nunca cobre textos, botão ou faixa do QTE.
                parent = null;
                foreach (var image in Object.FindObjectsByType<RawImage>(FindObjectsInactive.Include))
                    if (image.name == "LighthouseMechanismArt") parent = image.transform;
                if (parent == null) throw new System.Exception("Ilustração do mecanismo não encontrada.");
                host.transform.SetParent(parent, false);
                var hostRect = (RectTransform)host.transform;
                hostRect.anchorMin = hostRect.anchorMax = Vector2.one * .5f;
                hostRect.anchoredPosition = Vector2.zero;
                hostRect.sizeDelta = Vector2.zero;
            }
            else
            {
                var canvas = host.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = -1;
                var scaler = host.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = .5f;
                parent = host.transform;
            }
            var rect = new GameObject("StormFigure", typeof(RectTransform), typeof(CanvasGroup), typeof(Image)).GetComponent<RectTransform>();
            rect.SetParent(finale ? host.transform : parent, false);
            rect.anchorMin = rect.anchorMax = finale ? Vector2.one * .5f : name == "Beach" ? new Vector2(.65f,.58f) : new Vector2(.79f,.57f);
            rect.pivot = Vector2.one * .5f;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = finale ? new Vector2(195,255) : name == "Beach" ? new Vector2(110,155) : new Vector2(210,280);
            var imageComponent = rect.GetComponent<Image>();
            imageComponent.sprite = sprite;
            imageComponent.preserveAspect = true;
            imageComponent.raycastTarget = false;
            var group = rect.GetComponent<CanvasGroup>();
            group.alpha = 0;
            group.blocksRaycasts = group.interactable = false;
            var data = new SerializedObject(controller);
            data.FindProperty("setting").enumValueIndex = name == "Beach" ? 0 : name == "Cave" ? 1 : 2;
            Reference(data, "apparition", group); Reference(data, "figure", rect);
            Reference(data, "player", Object.FindAnyObjectByType<PlayerClickMovement>());
            Reference(data, "location", Object.FindAnyObjectByType<StoryLocation>());
            Reference(data, "lighthouse", Object.FindAnyObjectByType<LighthouseInteraction>());
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        // O validador existe apenas na cópia de testes, nunca na build do jogo.
        if (Application.isBatchMode)
        {
            var validator = System.Type.GetType("StormPursuitValidation, Assembly-CSharp");
            if (validator == null) throw new System.Exception("Execute a validação na cópia de testes.");
            validator.GetMethod("Run").Invoke(null, null);
        }
    }

    private static void BuildPursuit(Sprite sprite)
    {
        var root = new GameObject("StormPursuit");
        var pursuit = root.AddComponent<StormPursuit>();
        var player = Object.FindAnyObjectByType<PlayerClickMovement>();
        var ghost = new GameObject("ChasingShadow").AddComponent<SpriteRenderer>();
        ghost.transform.SetParent(root.transform);
        ghost.sprite = sprite;
        ghost.transform.localScale = Vector3.one * (1.65f / sprite.bounds.size.y);
        var playerSprite = player.GetComponent<SpriteRenderer>();
        ghost.sharedMaterial = playerSprite.sharedMaterial;
        ghost.sortingLayerID = playerSprite.sortingLayerID;
        ghost.sortingOrder = playerSprite.sortingOrder + 1;
        ghost.enabled = false;

        var ui = new GameObject("StormWarningCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        ui.transform.SetParent(root.transform);
        var canvas = ui.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1;
        var scaler = ui.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;
        var board = new GameObject("StormWarning", typeof(RectTransform),typeof(CanvasGroup),typeof(Image)).GetComponent<RectTransform>();
        board.SetParent(ui.transform,false); board.anchorMin=board.anchorMax=new Vector2(.5f,1);
        board.pivot=new Vector2(.5f,1); board.anchoredPosition=new Vector2(0,-30); board.sizeDelta=new Vector2(900,85);
        var paper=board.GetComponent<Image>(); paper.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/DialoguePaper-v2.png");
        paper.type=Image.Type.Sliced;paper.pixelsPerUnitMultiplier=3;paper.raycastTarget=false;
        var group=board.GetComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=group.interactable=false;
        var text=new GameObject("WarningText",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(board,false);text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;
        text.rectTransform.offsetMin=new Vector2(35,12);text.rectTransform.offsetMax=new Vector2(-35,-12);
        text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/FontesOriginais/DynaPuff-VariableFont_wdth,wght SDF.asset");
        text.fontSize=23;text.color=new Color(.15f,.24f,.25f);text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
        var data=new SerializedObject(pursuit);
        Reference(data,"player",player);Reference(data,"location",Object.FindAnyObjectByType<StoryLocation>());
        Reference(data,"shadow",ghost);Reference(data,"message",text);Reference(data,"messageGroup",group);
        data.ApplyModifiedPropertiesWithoutUndo();
    }
}
