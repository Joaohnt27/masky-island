using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class AuthorCrystalPuzzle
{
    private static TMP_FontAsset font;
    private static readonly Color cream = new Color(1f, 0.93f, 0.75f);
    private static GameObject Named(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            if (go.name == name) return go;
        throw new Exception("Missing " + name);
    }

    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        font = Named("PuzzleFeedback").GetComponent<TMP_Text>().font;
        var panel = Named("MinigPanel");
        if (panel.transform.Find("CrystalTriangleButton") != null) throw new Exception("Already authored");
        var ui = Named("MiningUI").GetComponent<MiningUI>();
        panel.transform.SetParent(Named("GameplayMenusCanvas").transform, false);
        panel.transform.SetAsFirstSibling();
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-20, 20);
        rect.sizeDelta = new Vector2(640, 600);
        panel.GetComponent<Image>().color = new Color(0.10f, 0.16f, 0.19f, 0.98f);
        panel.GetComponent<Image>().sprite = null;
        panel.GetComponent<Image>().raycastTarget = true;
        Text(Named("MinigTitle").GetComponent<TMP_Text>(), "OS CRISTAIS DA TEMPESTADE", 245, 42, 29);
        Label(panel.transform, "CrystalInstructions", "Selecione um veio e minere por 3 segundos.\nDepois, selecione o cristal e escolha seu encaixe.", 190, 64, 22);
        var triangle = Button(panel.transform, "CrystalTriangleButton", "TRIÂNGULO", -205, 120, 66, ui.SelectTriangle);
        var circle = Button(panel.transform, "CrystalCircleButton", "CÍRCULO", 0, 120, 66, ui.SelectCircle);
        var square = Button(panel.transform, "CrystalSquareButton", "QUADRADO", 205, 120, 66, ui.SelectSquare);
        var mine = Named("MinigButton").GetComponent<Button>();
        Place(mine.GetComponent<RectTransform>(), 0, 50, 240, 48);
        var mineLabel = mine.GetComponentInChildren<TMP_Text>();
        Text(mineLabel, "MINERAR", 0, 44, 24, 230);
        var progress = Named("MinigProgressBar").GetComponent<Slider>();
        Place(progress.GetComponent<RectTransform>(), 0, 7, 580, 18);
        var first = Button(panel.transform, "CrystalSocket1", "1\nSEM CANTOS", -205, -62, 80, ui.PlaceFirst);
        var second = Button(panel.transform, "CrystalSocket2", "2\nTRÊS PONTAS", 0, -62, 80, ui.PlaceSecond);
        var third = Button(panel.transform, "CrystalSocket3", "3\nQUATRO LADOS", 205, -62, 80, ui.PlaceThird);
        var inventory = Label(panel.transform, "CrystalInventory", "", -128, 40, 19);
        Text(Named("MinigResultText").GetComponent<TMP_Text>(), "", -190, 82, 22);
        var close = Button(panel.transform, "CloseCrystalPanel", "FECHAR", 0, -257, 42, ui.ClosePanel);
        Place(close.GetComponent<RectTransform>(), 0, -257, 180, 42);
        var data = new SerializedObject(ui);
        SetButtons(data, "crystalButtons", triangle, circle, square);
        SetButtons(data, "socketButtons", first, second, third);
        data.FindProperty("inventoryText").objectReferenceValue = inventory;
        var hint = Named("MiningAreaHint");
        hint.transform.SetParent(Named("GameplayMenusCanvas").transform, false);
        hint.transform.SetAsFirstSibling();
        var hintRect = hint.GetComponent<RectTransform>();
        hintRect.anchorMin = hintRect.anchorMax = hintRect.pivot = new Vector2(0.5f, 1);
        hintRect.anchoredPosition = new Vector2(0, -20);
        hintRect.sizeDelta = new Vector2(1040, 48);
        var hintText = hint.GetComponent<TMP_Text>();
        Style(hintText, 23);
        hintText.text = "PRÓXIMA PISTA: cristais na parte inferior direita da ilha.";
        data.FindProperty("areaHint").objectReferenceValue = hint;
        data.ApplyModifiedPropertiesWithoutUndo();
        Named("MiningSpot").SetActive(true);
        ui.gameObject.SetActive(true);
        panel.SetActive(false);
        hint.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CRYSTAL PUZZLE AUTHORED");
    }

    private static void SetButtons(SerializedObject data, string field, params Button[] buttons)
    {
        var array = data.FindProperty(field);
        array.arraySize = buttons.Length;
        for (int i = 0; i < buttons.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
    }
    private static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }
    private static void Style(TMP_Text text, float size)
    {
        text.font = font;
        text.fontSharedMaterial = font.material;
        text.fontSize = size;
        text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center;
        text.color = cream;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
    }
    private static void Text(TMP_Text text, string content, float y, float height, float size, float width = 600)
    {
        Place(text.rectTransform, 0, y, width, height);
        Style(text, size);
        text.text = content;
    }
    private static TMP_Text Label(Transform parent, string name, string content, float y, float height, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        Text(text, content, y, height, size);
        return text;
    }
    private static Button Button(Transform parent, string name, string content, float x, float y, float height, UnityAction action)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = 5;
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), x, y, 185, height);
        var image = go.GetComponent<Image>();
        image.color = new Color(0.23f, 0.36f, 0.40f);
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        var label = Label(go.transform, "Label", content, 0, height - 4, 21);
        label.rectTransform.sizeDelta = new Vector2(175, height - 4);
        UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }
}
