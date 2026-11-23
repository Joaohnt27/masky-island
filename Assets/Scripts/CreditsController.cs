using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private RectTransform viewport;
    [SerializeField, Min(1f)] private float scrollSpeed = 45f;
    [SerializeField, Min(0f)] private float initialDelay = 1f;
    [SerializeField, Min(0f)] private float finalDelay = 2f;
    [SerializeField] private bool returnAutomatically = true;
    private bool leaving;

    private IEnumerator Start()
    {
        if (creditsText == null || viewport == null) yield break;
        Canvas.ForceUpdateCanvases();
        RectTransform content = creditsText.rectTransform;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            creditsText.GetPreferredValues(creditsText.text, content.rect.width, Mathf.Infinity).y);
        // Pivot superior: começa abaixo da janela e termina quando a última linha saiu.
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, -viewport.rect.height * 0.5f);
        while (SceneFade.IsRunning) yield return null;
        yield return new WaitForSecondsRealtime(initialDelay);
        while (!leaving && content.anchoredPosition.y < viewport.rect.height * 0.5f + content.rect.height)
        {
            if (!SceneFade.IsRunning)
                content.anchoredPosition += Vector2.up * (Mathf.Max(1f, scrollSpeed) * Time.unscaledDeltaTime);
            yield return null;
        }
        if (!leaving && returnAutomatically)
        {
            yield return new WaitForSecondsRealtime(finalDelay);
            BackToMenu();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            BackToMenu();
    }

    public void BackToMenu()
    {
        if (leaving || !SceneFade.TryLoad("MainMenu")) return;
        leaving = true;
        MainMenuController.ReturningFromCredits = true;
    }
}
