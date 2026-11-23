using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class AuthorGameplayUI
{
    private static TMP_FontAsset font;
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var source = Object.FindAnyObjectByType<SettingsMenuUI>();
        font = (TMP_FontAsset)new SerializedObject(source).FindProperty("font").objectReferenceValue;
        foreach (string name in new[] { "Cave", "Island" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            if (scene.GetRootGameObjects().Any(go => go.GetComponent<PauseController>() != null))
                throw new System.Exception("Pause UI already exists in " + name);
            var result = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<MissionResultUI>(true)).Single();
            var resultData = new SerializedObject(result);
            var oldTitle = (TMP_Text)resultData.FindProperty("resultText").objectReferenceValue;

            var canvasObject = new GameObject("GameplayMenusCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var endPanel = Panel("MissionResultPanel", canvasObject.transform);
            var endCard = Card(endPanel.transform);
            oldTitle.name = "EndGameTitle";
            oldTitle.transform.SetParent(endCard, false);
            var rect = oldTitle.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 170);
            rect.sizeDelta = new Vector2(820, 75);
            oldTitle.alignment = TextAlignmentOptions.Center;
            oldTitle.text = "MISSÃO CUMPRIDA!";
            oldTitle.fontSize = 42;
            oldTitle.gameObject.SetActive(true);
            var message = Label("EndGameMessage", endCard, "Resultado da missão", new Vector2(0, 40), new Vector2(800, 155), 28);
            Button("RestartButton", endCard, "RECOMEÇAR", -105, result.Restart);
            Button("MainMenuButton", endCard, "MENU PRINCIPAL", -190, result.MainMenu);
            resultData.FindProperty("resultPanel").objectReferenceValue = endPanel;
            resultData.FindProperty("resultMessage").objectReferenceValue = message;
            resultData.ApplyModifiedPropertiesWithoutUndo();
            endPanel.SetActive(false);

            var pausePanel = Panel("PausePanel", canvasObject.transform);
            var pauseCard = Card(pausePanel.transform);
            Label("PauseTitle", pauseCard, "PAUSADO", new Vector2(0, 170), new Vector2(800, 80), 42);
            var host = new GameObject("GameplayMenuController");
            var pause = host.AddComponent<PauseController>();
            Button("ResumeButton", pauseCard, "CONTINUAR", 45, pause.Resume);
            Button("PauseSettingsButton", pauseCard, "CONFIGURAÇÕES", -45, pause.OpenSettings);
            Button("PauseMainMenuButton", pauseCard, "MENU PRINCIPAL", -135, pause.MainMenu);
            var settings = CopySettings(source, host, pausePanel);
            var pauseData = new SerializedObject(pause);
            pauseData.FindProperty("pausePanel").objectReferenceValue = pausePanel;
            pauseData.FindProperty("settingsUI").objectReferenceValue = settings;
            pauseData.ApplyModifiedPropertiesWithoutUndo();
            pausePanel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
        }
        Debug.Log("GAMEPLAY_UI_COMPLETE");
    }

    private static SettingsMenuUI CopySettings(SettingsMenuUI source, GameObject host, GameObject returnPanel)
    {
        var sourceData = new SerializedObject(source);
        var sourcePanel = (GameObject)sourceData.FindProperty("settingsPanel").objectReferenceValue;
        var panel = Object.Instantiate(sourcePanel, host.transform);
        panel.name = "SettingsPanel";
        panel.GetComponent<Canvas>().sortingOrder = 250;
        var map = new Dictionary<Object, Object>();
        var originals = sourcePanel.GetComponentsInChildren<Transform>(true);
        var copies = panel.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < originals.Length; i++)
        {
            map[originals[i].gameObject] = copies[i].gameObject;
            var a = originals[i].GetComponents<Component>();
            var b = copies[i].GetComponents<Component>();
            for (int j = 0; j < a.Length; j++) map[a[j]] = b[j];
        }
        var settings = host.AddComponent<SettingsMenuUI>();
        EditorUtility.CopySerialized(source, settings);
        var data = new SerializedObject(settings);
        var property = data.GetIterator();
        while (property.Next(true))
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue != null && map.TryGetValue(property.objectReferenceValue, out var copy))
                property.objectReferenceValue = copy;
        data.FindProperty("mainMenuPanel").objectReferenceValue = returnPanel;
        data.ApplyModifiedPropertiesWithoutUndo();
        // Instruções específicas do gameplay, mantendo os demais controles reais.
        var pages = data.FindProperty("pages");
        var controls = (GameObject)pages.GetArrayElementAtIndex(2).objectReferenceValue;
        foreach (var text in controls.GetComponentsInChildren<TMP_Text>(true))
            text.text = "MOUSE ESQUERDO\nClique no chão para mover e nos botões para selecionar.\n\nE\nInteragir com NPCs e pergaminhos próximos.\n\nESC\nPausar/continuar. Nas configurações, fechar listas ou voltar ao Pause.";
        panel.SetActive(false);
        return settings;
    }

    private static GameObject Panel(string name, Transform parent)
    {
        var rect = Rect(name, parent, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.gameObject.AddComponent<Image>().color = new Color(0.015f, 0.025f, 0.045f, 0.94f);
        return rect.gameObject;
    }
    private static RectTransform Card(Transform parent)
    {
        var card = Rect("Card", parent, Vector2.zero, new Vector2(960, 540));
        card.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.17f, 0.19f);
        return card;
    }
    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        return rect;
    }
    private static TMP_Text Label(string name, Transform parent, string content, Vector2 position, Vector2 size, float fontSize)
    {
        var text = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.text = content; text.fontSize = fontSize;
        text.color = new Color(0.96f, 0.88f, 0.72f);
        text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        return text;
    }
    private static void Button(string name, Transform parent, string title, float y, UnityAction action)
    {
        var rect = Rect(name, parent, new Vector2(0, y), new Vector2(420, 62));
        var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(0.77f, 0.57f, 0.29f);
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        Label("Label", rect, title, Vector2.zero, new Vector2(410, 60), 27);
        UnityEventTools.AddPersistentListener(button.onClick, action);
    }
}
