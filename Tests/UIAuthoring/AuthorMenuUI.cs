using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Executar somente na cópia de trabalho. Salva UI real com referências serializadas.
public static class AuthorMenuUI
{
    public static void Run()
    {
        var menuScene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        Object.FindAnyObjectByType<SettingsMenuUI>().BuildEditablePanel();
        var creditsButton = menu.mainMenuPanel.GetComponentsInChildren<Button>(true)
            .Single(button => button.name == "CreditsButton");
        if (creditsButton.onClick.GetPersistentEventCount() == 0)
            UnityEventTools.AddPersistentListener(creditsButton.onClick, menu.OpenCredits);
        EditorSceneManager.MarkSceneDirty(menuScene);
        EditorSceneManager.SaveScene(menuScene);

        const string path = "Assets/Scenes/Credits.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
        {
            var cameraTemplate = Camera.main.gameObject;
            var eventsTemplate = Object.FindAnyObjectByType<EventSystem>().gameObject;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            Object.Instantiate(cameraTemplate).name = "Main Camera";
            Object.Instantiate(eventsTemplate).name = "EventSystem";
            var root = new GameObject("CreditsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var background = Rect("Background", root.transform, Vector2.zero, Vector2.zero);
            background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
            background.gameObject.AddComponent<Image>().color = new Color(0.07f, 0.1f, 0.13f);
            Text("Title", root.transform, "CRÉDITOS", new Vector2(0, 300), new Vector2(1000, 70), 44);
            var viewport = Rect("CreditsViewport", root.transform, Vector2.zero, new Vector2(1000, 510));
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Text("CreditsText", viewport,
                "<size=42>MASK ISLAND</size>\n\nUm jogo acadêmico desenvolvido em Unity\n\n\n" +
                "<b>EQUIPE</b>\n[Nome do integrante]\n[Nome do integrante]\n[Nome do integrante]\n\n\n" +
                "<b>PROGRAMAÇÃO</b>\n[Nomes e contribuições]\n\n\n" +
                "<b>ARTE E ANIMAÇÃO</b>\n[Nomes e contribuições]\n\n\n" +
                "<b>ÁUDIO E MÚSICA</b>\n[Autores e fontes utilizadas]\n\n\n" +
                "<b>ORIENTAÇÃO</b>\n[Professor ou professora]\n[Instituição / curso]\n\n\n" +
                "<b>ASSETS E AGRADECIMENTOS</b>\n[Créditos e licenças dos assets]\n[Agradecimentos]\n\n\n" +
                "OBRIGADO POR JOGAR!", new Vector2(0, 240), new Vector2(920, 1700), 28);
            text.alignment = TextAlignmentOptions.Top;
            text.rectTransform.pivot = new Vector2(0.5f, 1f);
            var controller = new GameObject("CreditsController").AddComponent<CreditsController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("creditsText").objectReferenceValue = text;
            serialized.FindProperty("viewport").objectReferenceValue = viewport;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var back = Rect("BackButton", root.transform, new Vector2(0, -305), new Vector2(270, 54));
            var image = back.gameObject.AddComponent<Image>();
            image.color = new Color(0.77f, 0.57f, 0.29f);
            var button = back.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text("Label", back, "VOLTAR", Vector2.zero, new Vector2(260, 50), 25);
            UnityEventTools.AddPersistentListener(button.onClick, controller.BackToMenu);
            EditorSceneManager.SaveScene(scene, path);
        }
        var builds = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!builds.Any(scene => scene.path == path)) builds.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = builds.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("UI_AUTHORING_COMPLETE");
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TMP_Text Text(string name, Transform parent, string content, Vector2 position, Vector2 size, float fontSize)
    {
        var text = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("8f586378b4e144a9851e7b34d9b748ee"));
        text.text = content;
        text.fontSize = fontSize;
        text.color = new Color(0.96f, 0.88f, 0.72f);
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }
}
