using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Controla o painel salvo na cena. A aparência é editada na Hierarchy/Inspector.
public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject[] pages = new GameObject[3];
    [SerializeField] private Button[] tabs = new Button[3];
    [SerializeField] private Slider master, music, effects;
    [SerializeField] private TMP_Text[] percentages = new TMP_Text[3];
    [SerializeField] private Button applyButton, backButton;
    [SerializeField] private TMP_Dropdown resolutions, screenMode, quality;
    [SerializeField] private TMP_Text videoFeedback;
    private readonly Color ink = new Color(0.07f, 0.10f, 0.13f);
    private readonly Color paper = new Color(0.96f, 0.88f, 0.72f);
    private readonly Color accent = new Color(0.77f, 0.57f, 0.29f);
    private MainMenuController menu;
    private PauseController pause;

    public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;
    public bool IsTransitioning => menu != null && menu.IsTransitioning;

    private void Awake()
    {
        menu = GetComponent<MainMenuController>();
        pause = GetComponent<PauseController>();
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsPanel não foi atribuído no Inspector.", this);
            enabled = false;
            return;
        }
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].onClick.AddListener(() => ShowPage(index));
        }
        master.onValueChanged.AddListener(value => { SettingsManager.Instance.SetMasterVolume(value); UpdatePercent(master); });
        music.onValueChanged.AddListener(value => { SettingsManager.Instance.SetMusicVolume(value); UpdatePercent(music); });
        effects.onValueChanged.AddListener(value => { SettingsManager.Instance.SetSFXVolume(value); UpdatePercent(effects); });
        applyButton.onClick.AddListener(ApplyVideo);
        backButton.onClick.AddListener(Close);
        settingsPanel.SetActive(false);
    }

    public void Open()
    {
        if ((menu == null && pause == null) || IsTransitioning || SceneFade.IsRunning || SettingsManager.Instance == null || settingsPanel == null || mainMenuPanel == null || !mainMenuPanel.activeSelf)
            return;
        var settings = SettingsManager.Instance;
        master.SetValueWithoutNotify(settings.MasterVolume);
        music.SetValueWithoutNotify(settings.MusicVolume);
        effects.SetValueWithoutNotify(settings.SFXVolume);
        UpdatePercent(master); UpdatePercent(music); UpdatePercent(effects);
        settings.RefreshResolutions();
        var options = new List<string>();
        foreach (Vector2Int size in settings.Resolutions)
            options.Add($"{size.x} × {size.y}");
        resolutions.ClearOptions();
        resolutions.AddOptions(options);
        resolutions.SetValueWithoutNotify(settings.ResolutionIndex);
        screenMode.SetValueWithoutNotify(settings.Fullscreen ? 0 : 1);
        quality.ClearOptions();
        quality.AddOptions(new List<string>(QualitySettings.names));
        quality.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        videoFeedback.text = "Selecione as opções e clique em APLICAR.";
        if (menu != null) menu.TrySwitchPanels(mainMenuPanel, settingsPanel, () => ShowPage(0));
        else
        {
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
            ShowPage(0);
        }
    }

    public void Close()
    {
        if (!IsOpen || IsTransitioning || SceneFade.IsRunning) return;
        SettingsManager.Instance.Save();
        resolutions.Hide(); screenMode.Hide(); quality.Hide();
        if (menu != null) menu.TrySwitchPanels(settingsPanel, mainMenuPanel);
        else
        {
            settingsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void Update()
    {
        if (pause != null) return; // ESC é tratado uma única vez pelo PauseController.
        if (!IsOpen || IsTransitioning || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;
        HandleEscape();
    }

    public void HandleEscape()
    {
        if (!IsOpen || IsTransitioning) return;
        // ESC fecha primeiro a lista de um dropdown; um novo ESC volta ao menu.
        if (resolutions.IsExpanded || screenMode.IsExpanded || quality.IsExpanded)
        {
            resolutions.Hide(); screenMode.Hide(); quality.Hide();
            return;
        }
        Close();
    }

    public void HideForGameplayState()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
            tabs[i].interactable = i != index;
        }
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(tabs[index].gameObject);
    }

    private void ApplyVideo()
    {
        SettingsManager.Instance.ApplyVideo(resolutions.value, screenMode.value == 0, quality.value);
        videoFeedback.text = "Configurações de vídeo aplicadas.";
    }

    private void UpdatePercent(Slider slider)
    {
        int index = slider == master ? 0 : slider == music ? 1 : 2;
        if (percentages[index] != null)
            percentages[index].text = $"{Mathf.RoundToInt(slider.value * 100)}%";
    }

#if UNITY_EDITOR
    // Ferramenta de criação única. Nunca recria a aparência durante o jogo.
    [ContextMenu("Criar painel editável (se ausente)")]
    public void BuildEditablePanel()
    {
        if (settingsPanel != null) return;
        settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        settingsPanel.transform.SetParent(transform, false);
        var canvas = settingsPanel.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = settingsPanel.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var backdrop = Rect("SettingsBackground", settingsPanel.transform, Vector2.zero, Vector2.zero);
        backdrop.anchorMin = Vector2.zero; backdrop.anchorMax = Vector2.one;
        backdrop.gameObject.AddComponent<Image>().color = ink;
        var card = Rect("SettingsCard", settingsPanel.transform, Vector2.zero, new Vector2(1000, 620));
        card.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.17f, 0.19f);
        Label(card, "CONFIGURAÇÕES", new Vector2(0, 252), new Vector2(900, 65), 42);
        string[] titles = { "ÁUDIO", "VÍDEO", "CONTROLES" };
        for (int i = 0; i < titles.Length; i++)
        {
            int page = i;
            tabs[i] = MakeButton(card, titles[i], new Vector2((i - 1) * 290, 168),
                new Vector2(270, 54), () => ShowPage(page));
            pages[i] = Rect(titles[i], card, new Vector2(0, -25), new Vector2(880, 300)).gameObject;
        }
        master = AudioSlider(pages[0].transform, "Volume Geral", 90, value => SettingsManager.Instance.SetMasterVolume(value));
        music = AudioSlider(pages[0].transform, "Música", 0, value => SettingsManager.Instance.SetMusicVolume(value));
        effects = AudioSlider(pages[0].transform, "Efeitos Sonoros", -90, value => SettingsManager.Instance.SetSFXVolume(value));
        resolutions = Dropdown(pages[1].transform, "Resolução", 95);
        screenMode = Dropdown(pages[1].transform, "Modo de tela", 20);
        screenMode.AddOptions(new List<string> { "Fullscreen", "Janela" });
        quality = Dropdown(pages[1].transform, "Qualidade gráfica", -55);
        applyButton = MakeButton(pages[1].transform, "APLICAR", new Vector2(280, -140), new Vector2(230, 48), ApplyVideo);
        videoFeedback = Label(pages[1].transform, "", new Vector2(-150, -140), new Vector2(530, 65), 21);
        Label(pages[2].transform,
            "W A S D\nMovimentar o personagem.\n\n" +
            "MOUSE ESQUERDO\nInteragir com objetos e caminhos. I abre o inventário.\n\n" +
            "ESC\nPausar ou fechar o inventário.",
            Vector2.zero, new Vector2(840, 300), 25);
        backButton = MakeButton(card, "VOLTAR", new Vector2(0, -252), new Vector2(270, 54), Close);
        percentages[0] = master.transform.Find("Percent").GetComponent<TMP_Text>();
        percentages[1] = music.transform.Find("Percent").GetComponent<TMP_Text>();
        percentages[2] = effects.transform.Find("Percent").GetComponent<TMP_Text>();
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == 0);
        settingsPanel.SetActive(false);
        UnityEditor.EditorUtility.SetDirty(this);
    }

    private RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private TMP_Text Label(Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
    {
        var label = Rect("Text", parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.color = paper;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private Button MakeButton(Transform parent, string text, Vector2 position, Vector2 size, UnityAction action)
    {
        var rect = Rect(text, parent, position, size);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = accent;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        Label(rect, text, Vector2.zero, size - new Vector2(14, 4), 25);
        return button;
    }

    private Slider AudioSlider(Transform parent, string title, float y, UnityAction<float> changed)
    {
        Label(parent, title, new Vector2(-285, y), new Vector2(290, 60), 27);
        var control = DefaultControls.CreateSlider(new DefaultControls.Resources());
        control.name = title;
        control.transform.SetParent(parent, false);
        var rect = control.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(90, y);
        rect.sizeDelta = new Vector2(390, 30);
        var slider = control.GetComponent<Slider>();
        slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
        slider.fillRect.GetComponent<Image>().color = accent;
        var percent = Label(control.transform, "100%", new Vector2(250, 0), new Vector2(90, 40), 24);
        percent.name = "Percent";
        return slider;
    }

    private TMP_Dropdown Dropdown(Transform parent, string title, float y)
    {
        Label(parent, title, new Vector2(-275, y), new Vector2(310, 55), 27);
        var control = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
        control.name = title;
        control.transform.SetParent(parent, false);
        var rect = control.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(160, y);
        rect.sizeDelta = new Vector2(480, 52);
        foreach (TMP_Text text in control.GetComponentsInChildren<TMP_Text>(true))
        {
            if (font != null) text.font = font;
            text.fontSize = 25;
            text.color = ink;
        }
        var dropdown = control.GetComponent<TMP_Dropdown>();
        var item = dropdown.itemText.transform.parent.GetComponent<RectTransform>();
        item.sizeDelta = new Vector2(item.sizeDelta.x, 44);
        dropdown.template.sizeDelta = new Vector2(dropdown.template.sizeDelta.x, 240);
        dropdown.ClearOptions();
        return dropdown;
    }

#endif

    private void OnDisable()
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.Save();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
