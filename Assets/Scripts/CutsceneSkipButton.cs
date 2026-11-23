using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Controle compartilhado pelas apresentações, independente dos painéis bloqueados.
public class CutsceneSkipButton : MonoBehaviour
{
    private Action skip;
    private Button button;

    public static CutsceneSkipButton Create(Transform owner, TMP_FontAsset font, Action action)
    {
        var root = new GameObject("CutsceneSkip", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, owner.gameObject.scene);
        if (EventSystem.current == null)
        {
            var events = new GameObject("CutsceneInput", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(root.transform, false);
        }
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32761;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;
        var face = new GameObject("SkipAnimationButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        face.transform.SetParent(root.transform, false);
        var rect = (RectTransform)face.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-28, 28);
        rect.sizeDelta = new Vector2(244, 54);
        var surface = face.GetComponent<Image>();
        surface.color = new Color(.07f, .22f, .23f, .97f);
        var edge = face.GetComponent<Outline>();
        edge.effectColor = new Color(.78f, .61f, .34f);
        edge.effectDistance = new Vector2(2, -2);
        var label = new GameObject("SkipAnimationLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(face.transform, false);
        var text = label.GetComponent<TextMeshProUGUI>();
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = new Vector2(10, 4);
        text.rectTransform.offsetMax = new Vector2(-10, -4);
        text.font = font;
        text.text = "PULAR  <size=13>[ESC]</size>  >";
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(.98f, .91f, .73f);
        text.raycastTarget = false;
        var control = root.AddComponent<CutsceneSkipButton>();
        control.skip = action;
        control.button = face.GetComponent<Button>();
        control.button.targetGraphic = surface;
        var colors = control.button.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.15f);
        colors.pressedColor = new Color(.75f, .85f, .8f);
        control.button.colors = colors;
        control.button.onClick.AddListener(control.Activate);
        root.SetActive(false);
        return control;
    }

    public void Show(bool visible) { gameObject.SetActive(visible); button.interactable = visible; }
    private void Activate()
    {
        if (!button.interactable || SceneFade.IsRunning) return;
        Show(false);
        skip?.Invoke();
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Activate();
    }
}
