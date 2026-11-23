using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class RedesignCrystalHUD
{
    private static TMP_FontAsset body;
    private static Sprite round;
    private static Color cream = new Color(0.96f, 0.94f, 0.85f);
    private static Color gold = new Color(0.96f, 0.77f, 0.4f);
    private static Color surface = new Color(0.10f, 0.18f, 0.21f);
    private static GameObject Named(string name)
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include)) if (go.name == name) return go;
        throw new Exception(name);
    }
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Island.unity");
        body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        round = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var panel = Named("MinigPanel");
        var root = panel.GetComponent<RectTransform>();
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(1, 0.5f);
        root.anchoredPosition = new Vector2(-20, 0);
        root.sizeDelta = new Vector2(596, 650);
        Skin(panel.GetComponent<Image>(), new Color(0.035f, 0.075f, 0.095f, 0.99f));
        var rim = Rect("FrameInset", root, 0, 0, 590, 644);
        Skin(rim.gameObject.AddComponent<Image>(), surface);
        rim.SetAsFirstSibling();
        var accent = Rect("HeaderAccent", root, 0, 320, 120, 3);
        Skin(accent.gameObject.AddComponent<Image>(), gold);
        Label(root, "MemoryEyebrow", "LEMBRANÇA 02  /  A ILHA DOS ESQUECIDOS", 0, 293, 520, 22, 13, gold);
        var title = Named("MinigTitle").GetComponent<TMP_Text>();
        Place(title.rectTransform, 0, 260, 530, 48);
        title.text = "Ecos da tempestade";
        title.fontSize = 32;
        title.color = cream;
        var markers = new Image[3];
        string[] steps = { "1  COLETAR", "2  ENCAIXAR", "3  LEMBRAR" };
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 178;
            Label(root, "StepLabel" + i, steps[i], x, 211, 166, 30, 15, cream);
            markers[i] = Rect("StepMarker" + i, root, x, 188, 154, 3).gameObject.AddComponent<Image>();
        }
        var heading = Label(root, "CrystalStepTitle", "Colete os três cristais", 0, 150, 530, 40, 27, cream);
        heading.fontStyle = FontStyles.Bold;
        var instruction = Named("CrystalInstructions").GetComponent<TMP_Text>();
        Text(instruction, 0, 108, 510, 48, 19);
        var cards = Rect("CrystalCards", root, 0, 0, 550, 144);
        var actions = Rect("CollectionActions", root, 0, 0, 550, 650);
        var sockets = Rect("CrystalSockets", root, 0, 0, 550, 650);
        var complete = Rect("MemoryReveal", root, 0, 0, 550, 650);
        var borders = new Image[3];
        Color[] crystalColors = { new Color(0.98f, 0.69f, 0.32f), new Color(0.36f, 0.83f, 0.85f), new Color(0.69f, 0.58f, 0.98f) };
        string[] names = { "CrystalTriangleButton", "CrystalCircleButton", "CrystalSquareButton" };
        var symbols = new CrystalSymbolGraphic[3];
        for (int i = 0; i < 3; i++)
        {
            var button = Named(names[i]).GetComponent<Button>();
            button.transform.SetParent(cards, false);
            Place(button.GetComponent<RectTransform>(), (i - 1) * 178, 0, 166, 144);
            borders[i] = button.GetComponent<Image>();
            Skin(borders[i], gold);
            var backing = Rect("CardSurface", button.transform, 0, 0, 162, 140).gameObject.AddComponent<Image>();
            Skin(backing, new Color(0.075f, 0.13f, 0.16f));
            backing.transform.SetAsFirstSibling();
            button.targetGraphic = backing;
            var colors = button.colors;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.disabledColor = Color.white;
            button.colors = colors;
            Text(button.GetComponentInChildren<TMP_Text>(true), 0, -43, 156, 47, 19);
            Icon(button.transform, "CrystalShape", (GameManager.CrystalId)i, 0, 24, 76, crystalColors[i]);

            var slot = Named("CrystalSocket" + (i + 1)).GetComponent<Button>();
            slot.transform.SetParent(sockets, false);
            Place(slot.GetComponent<RectTransform>(), (i - 1) * 178, -132, 166, 100);
            Skin(slot.GetComponent<Image>(), new Color(0.07f, 0.12f, 0.14f));
            Text(slot.GetComponentInChildren<TMP_Text>(true), 0, -25, 156, 30, 18);
            Label(slot.transform, "SocketNumber", "ENCAIXE " + (i + 1), 0, 29, 150, 20, 12, new Color(0.60f, 0.72f, 0.73f));
            int shape = i == 0 ? 1 : i == 1 ? 0 : 2;
            symbols[i] = Icon(slot.transform, "PlacedShape", (GameManager.CrystalId)shape, 0, 0, 35, crystalColors[shape]);
            symbols[i].gameObject.SetActive(false);
            Icon(complete, "MemoryCrystal" + i, (GameManager.CrystalId)i, (i - 1) * 108, 5, 104, crystalColors[i]);
        }
        Label(complete, "MemoryNextStep", "Converse com o estranho sobre a tempestade.\nSua jornada continua.", 0, -208, 500, 55, 19, cream);
        var mine = Named("MinigButton").GetComponent<Button>();
        mine.transform.SetParent(actions, false);
        Place(mine.GetComponent<RectTransform>(), 0, -122, 510, 54);
        Skin(mine.GetComponent<Image>(), gold);
        var mineColors = ColorBlock.defaultColorBlock;
        mineColors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
        mine.colors = mineColors;
        var primary = mine.GetComponentInChildren<TMP_Text>(true);
        Text(primary, 0, 0, 490, 48, 21);
        primary.color = new Color(0.08f, 0.13f, 0.15f);
        primary.fontStyle = FontStyles.Bold;
        var bar = Named("MinigProgressBar").GetComponent<Slider>();
        bar.transform.SetParent(actions, false);
        Place(bar.GetComponent<RectTransform>(), 0, -166, 510, 8);
        if (bar.handleRect != null) bar.handleRect.gameObject.SetActive(false);
        if (bar.fillRect != null) Skin(bar.fillRect.GetComponent<Image>(), gold);
        var progressLabel = Label(actions, "ExtractionProgressLabel", "", 0, -190, 510, 24, 16, cream);
        var result = Named("MinigResultText").GetComponent<TMP_Text>();
        Text(result, 0, -226, 510, 58, 18);
        var inventory = Named("CrystalInventory").GetComponent<TMP_Text>();
        Text(inventory, 0, -265, 510, 25, 15);
        inventory.color = gold;
        var close = Named("CloseCrystalPanel").GetComponent<Button>();
        Place(close.GetComponent<RectTransform>(), 0, -302, 270, 34);
        Skin(close.GetComponent<Image>(), new Color(0.13f, 0.23f, 0.26f));
        var closeText = close.GetComponentInChildren<TMP_Text>(true);
        Text(closeText, 0, 0, 260, 30, 15);
        var data = new SerializedObject(Named("MiningUI").GetComponent<MiningUI>());
        Assign(data,"stepTitle",heading); Assign(data,"instructions",instruction); Assign(data,"primaryLabel",primary);
        Assign(data,"progressLabel",progressLabel); Assign(data,"closeLabel",closeText);
        Assign(data,"crystalGroup",cards.gameObject); Assign(data,"actionGroup",actions.gameObject);
        Assign(data,"socketGroup",sockets.gameObject); Assign(data,"completionGroup",complete.gameObject);
        Array(data,"stepMarkers",markers); Array(data,"selectionBorders",borders); Array(data,"socketSymbols",symbols);
        data.ApplyModifiedPropertiesWithoutUndo();
        panel.GetComponent<Image>().raycastTarget = true;
        foreach (var button in panel.GetComponentsInChildren<Button>(true))
            button.GetComponent<Image>().raycastTarget = true;
        panel.SetActive(false);
        sockets.gameObject.SetActive(false); complete.gameObject.SetActive(false);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("GUIDED CRYSTAL HUD SAVED");
    }
    private static void Assign(SerializedObject data,string field,Object value) => data.FindProperty(field).objectReferenceValue=value;
    private static void Array(SerializedObject data,string field,Object[] values)
    {
        var property=data.FindProperty(field); property.arraySize=values.Length;
        for(int i=0;i<values.Length;i++) property.GetArrayElementAtIndex(i).objectReferenceValue=values[i];
    }
    private static void Skin(Image image,Color color)
    {
        image.sprite=round; image.type=Image.Type.Sliced; image.color=color; image.raycastTarget=false;
    }
    private static void Place(RectTransform rect,float x,float y,float w,float h)
    {
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0.5f,0.5f);
        rect.anchoredPosition=new Vector2(x,y); rect.sizeDelta=new Vector2(w,h); rect.localScale=Vector3.one;
    }
    private static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
    {
        var go=new GameObject(name,typeof(RectTransform)); go.layer=5;
        go.transform.SetParent(parent,false); var rect=go.GetComponent<RectTransform>(); Place(rect,x,y,w,h); return rect;
    }
    private static void Text(TMP_Text text,float x,float y,float w,float h,float size)
    {
        Place(text.rectTransform,x,y,w,h); text.font=body; text.fontSharedMaterial=body.material;
        text.fontSize=size; text.enableAutoSizing=false; text.fontStyle=FontStyles.Normal;
        text.textWrappingMode=TextWrappingModes.Normal;
        text.alignment=TextAlignmentOptions.Center; text.color=cream; text.margin=Vector4.zero; text.raycastTarget=false;
    }
    private static TMP_Text Label(Transform parent,string name,string value,float x,float y,float w,float h,float size,Color tint)
    {
        var text=Rect(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();
        Text(text,x,y,w,h,size); text.text=value; text.color=tint; return text;
    }
    private static CrystalSymbolGraphic Icon(Transform parent,string name,GameManager.CrystalId shape,float x,float y,float size,Color tint)
    {
        var icon=Rect(name,parent,x,y,size,size).gameObject.AddComponent<CrystalSymbolGraphic>();
        icon.Shape=shape; icon.color=tint; icon.raycastTarget=false; return icon;
    }
}
