using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Executado somente na cópia isolada. A cena salva é copiada após a validação.
public static class AuthorFirstPuzzle
{
    private static TMP_FontAsset font;
    private static Color ink = new Color(0.24f, 0.14f, 0.09f);

    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        var scroll = Object.FindAnyObjectByType<ScrollInteraction>();
        var scrollData = new SerializedObject(scroll);
        var panel = (GameObject)scrollData.FindProperty("scrollPanel").objectReferenceValue;
        if (panel.GetComponent<SymbolSequencePuzzle>() != null)
            throw new InvalidOperationException("The first puzzle already exists.");

        var canvas = panel.GetComponentInParent<Canvas>();
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        panel.GetComponent<Image>().raycastTarget = true;

        var title = panel.transform.Find("ScrollTitle").GetComponent<TMP_Text>();
        font = title.font;
        ConfigureText(title, new Vector2(0, 190), new Vector2(700, 60), 36);
        title.text = "A INSCRIÇÃO DO CÉU";
        var clue = panel.transform.Find("ScrollText").GetComponent<TMP_Text>();
        ConfigureText(clue, new Vector2(0, 95), new Vector2(700, 110), 28);
        clue.text = "O sol se despede.\nA lua desperta.\nA estrela mostra o caminho.";

        var close = panel.transform.Find("CloseButton").GetComponent<Button>();
        Place(close.GetComponent<RectTransform>(), new Vector2(0, -230), new Vector2(200, 56));
        var closeLabel = close.GetComponentInChildren<TMP_Text>();
        ConfigureText(closeLabel, Vector2.zero, new Vector2(190, 50), 26);
        closeLabel.color = new Color(1f, 0.93f, 0.75f);

        var puzzle = panel.AddComponent<SymbolSequencePuzzle>();
        var star = Button(panel.transform, "StarSymbolButton", "ESTRELA", -205, puzzle.SelectStar);
        var sun = Button(panel.transform, "SunSymbolButton", "SOL", 0, puzzle.SelectSun);
        var moon = Button(panel.transform, "MoonSymbolButton", "LUA", 205, puzzle.SelectMoon);
        var feedback = Label(panel.transform, "PuzzleFeedback", new Vector2(0, -120), new Vector2(690, 140), 24);
        feedback.text = "Converse com o estranho de capuz vermelho antes de decifrar a inscrição.";
        var puzzleData = new SerializedObject(puzzle);
        puzzleData.FindProperty("sunButton").objectReferenceValue = sun;
        puzzleData.FindProperty("moonButton").objectReferenceValue = moon;
        puzzleData.FindProperty("starButton").objectReferenceValue = star;
        puzzleData.FindProperty("feedbackText").objectReferenceValue = feedback;
        puzzleData.ApplyModifiedPropertiesWithoutUndo();

        var prompt = (GameObject)scrollData.FindProperty("interactionPrompt").objectReferenceValue;
        prompt.GetComponentInChildren<TMP_Text>(true).text = "E - LER A INSCRIÇÃO";
        panel.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("FIRST PUZZLE AUTHORED: scene references and button callbacks saved.");
    }

    private static void Place(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void ConfigureText(TMP_Text text, Vector2 position, Vector2 size, float fontSize)
    {
        Place(text.rectTransform, position, size);
        text.font = font;
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center;
        text.color = ink;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.margin = Vector4.zero;
    }

    private static TMP_Text Label(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        ConfigureText(text, position, size, fontSize);
        return text;
    }

    private static Button Button(Transform parent, string name, string text, float x, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), new Vector2(x, -10), new Vector2(185, 58));
        var image = go.GetComponent<Image>();
        image.color = new Color(0.40f, 0.25f, 0.13f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.91f, 0.68f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.55f);
        button.colors = colors;
        var label = Label(go.transform, "Label", Vector2.zero, new Vector2(175, 50), 27);
        label.text = text;
        label.color = new Color(1f, 0.93f, 0.75f);
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }
}
