using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [SerializeField] private string nextScene = "MainMenu";
    [SerializeField] private Canvas splashCanvas;
    [SerializeField, Range(10, 100)] private int particleCount = 48;
    [SerializeField] private Color particleColor = new Color(1f, 0.8f, 0.4f, 0.7f);
    [SerializeField, Min(0.3f)] private float clickDuration = 0.85f;
    private SplashParticles particles;
    private bool loading;

    private void Awake()
    {
        if (splashCanvas == null) splashCanvas = FindFirstObjectByType<Canvas>();
        if (splashCanvas == null) return;
        var effect = new GameObject("Splash Particles", typeof(RectTransform), typeof(CanvasRenderer));
        effect.transform.SetParent(splashCanvas.transform, false);
        var rect = (RectTransform)effect.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        particles = effect.AddComponent<SplashParticles>();
        particles.Initialize(particleCount, particleColor);
    }

    private void Update()
    {
        if (!loading && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            ContinueToMenu();
    }

    public void ContinueToMenu()
    {
        if (loading)
            return;
        loading = true;
        StartCoroutine(AnimateAndContinue());
    }

    private IEnumerator AnimateAndContinue()
    {
        if (particles != null)
        {
            Vector2 point = Mouse.current != null ? Mouse.current.position.ReadValue() :
                new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Camera camera = splashCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : splashCanvas.worldCamera;
            particles.PlayClick(point, camera, clickDuration);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.3f, clickDuration));
        }
        SceneManager.LoadSceneAsync(nextScene);
    }
}
