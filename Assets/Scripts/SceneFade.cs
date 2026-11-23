using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Vive somente durante a troca de cena, mantendo a cobertura até o destino estar pronto.
public class SceneFade : MonoBehaviour
{
    public const float QuickDuration = 0.18f;
    private static SceneFade active;
    public static bool IsRunning => active != null;
    private Image shade;
    public static string OriginScene { get; private set; }

    public static bool TryLoad(string sceneName)
    {
        if (sceneName == "Lighthouse" && (GameManager.Instance == null || !GameManager.Instance.IsLighthouseUnlocked)) sceneName = "LighthouseExterior";
        if (IsRunning || string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
            return false;
        OriginScene = SceneManager.GetActiveScene().name;
        if (GameManager.IsExplorationScene(sceneName)) GameAudio.Play(GameAudio.Cue.Travel);
        var root = new GameObject("Scene Fade", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        active = root.AddComponent<SceneFade>();
        DontDestroyOnLoad(root);
        active.StartCoroutine(active.ChangeScene(sceneName));
        return true;
    }

    private void Awake()
    {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        var image = new GameObject("Scene Fade Shade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        image.transform.SetParent(transform, false);
        shade = image.GetComponent<Image>();
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        shade.color = new Color(0.015f, 0.025f, 0.045f, 0f);
        shade.raycastTarget = true;
    }

    private IEnumerator ChangeScene(string sceneName)
    {
        yield return FadeTo(1f);
        yield return SceneManager.LoadSceneAsync(sceneName);
        // Aguarda Start/layout da nova cena sem revelar um frame incompleto.
        yield return null;
        yield return FadeTo(0f);
        Destroy(gameObject);
    }

    private IEnumerator FadeTo(float target)
    {
        float from = shade.color.a;
        for (float elapsed = 0f; elapsed < QuickDuration; elapsed += Time.unscaledDeltaTime)
        {
            SetAlpha(Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, elapsed / QuickDuration)));
            yield return null;
        }
        SetAlpha(target);
    }

    private void SetAlpha(float alpha)
    {
        Color color = shade.color;
        color.a = alpha;
        shade.color = color;
    }

    private void OnDestroy()
    {
        if (active == this) active = null;
    }
}

