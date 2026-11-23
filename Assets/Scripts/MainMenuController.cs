using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour
{
    public static bool ReturningFromCredits { get; set; }
    public static bool ReturningFromGameplay { get; set; }
    [Header("Telas")]
    public GameObject nicknamePanel;
    public GameObject mainMenuPanel;

    [Header("Nickname")]
    public TMP_InputField nicknameInput;

    [Header("Transição")]
    public GameObject transitionPanel;
    public VideoPlayer transitionVideoPlayer;
    [SerializeField] private VideoClip gameplayIntroClip;
    [SerializeField, Min(0.05f)] private float fadeDuration = 0.47f;
    private Image fadeImage;
    private CanvasGroup nicknameControls, menuControls;
    private bool introFinished;
    public bool IsTransitioning { get; private set; }
    private bool enteringGameplay;
    private bool playingIntro;
    [SerializeField] private TMP_FontAsset skipButtonFont;
    private CutsceneSkipButton skipButton;

    private void Awake()
    {
        skipButton = CutsceneSkipButton.Create(transform, skipButtonFont, SkipIntro);
        if (ReturningFromCredits || ReturningFromGameplay)
        {
            ReturningFromCredits = false;
            ReturningFromGameplay = false;
            nicknamePanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            if (transitionPanel != null) transitionPanel.SetActive(false);
        }
        // Overlay local acima dos Canvas existentes, sem modificar a cena salva.
        var overlay = new GameObject("Menu Fade", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        var shade = new GameObject("Fade Shade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        shade.transform.SetParent(overlay.transform, false);
        fadeImage = shade.GetComponent<Image>();
        fadeImage.rectTransform.anchorMin = Vector2.zero;
        fadeImage.rectTransform.anchorMax = Vector2.one;
        fadeImage.rectTransform.offsetMin = fadeImage.rectTransform.offsetMax = Vector2.zero;
        fadeImage.color = new Color(0.015f, 0.025f, 0.045f, 1f);
        nicknameControls = nicknamePanel.GetComponent<CanvasGroup>();
        if (nicknameControls == null) nicknameControls = nicknamePanel.AddComponent<CanvasGroup>();
        menuControls = mainMenuPanel.GetComponent<CanvasGroup>();
        if (menuControls == null) menuControls = mainMenuPanel.AddComponent<CanvasGroup>();
        SetTransitioning(true);
        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.loopPointReached += OnTransitionVideoFinished;
            transitionVideoPlayer.errorReceived += OnTransitionError;
        }
    }

    private IEnumerator Start()
    {
        yield return FadeTo(0f);
        SetTransitioning(false);
    }

    private void SetTransitioning(bool value)
    {
        IsTransitioning = value;
        fadeImage.raycastTarget = value;
        nicknameControls.interactable = !value;
        menuControls.interactable = !value;
        if (value && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private IEnumerator FadeTo(float alpha)
    {
        float from = fadeImage.color.a;
        float duration = Mathf.Max(0.05f, fadeDuration);
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            Color shade = fadeImage.color;
            shade.a = Mathf.Lerp(from, alpha, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            fadeImage.color = shade;
            yield return null;
        }
        Color final = fadeImage.color;
        final.a = alpha;
        fadeImage.color = final;
    }

    public bool TrySwitchPanels(GameObject outgoing, GameObject incoming, System.Action onCovered = null)
    {
        if (IsTransitioning || SceneFade.IsRunning || outgoing == null || incoming == null)
            return false;
        SetTransitioning(true);
        StartCoroutine(SwitchPanels(outgoing, incoming, onCovered));
        return true;
    }

    private IEnumerator SwitchPanels(GameObject outgoing, GameObject incoming, System.Action onCovered)
    {
        var from = outgoing.GetComponent<CanvasGroup>();
        if (from == null) from = outgoing.AddComponent<CanvasGroup>();
        var to = incoming.GetComponent<CanvasGroup>();
        if (to == null) to = incoming.AddComponent<CanvasGroup>();
        from.interactable = to.interactable = false;
        yield return FadeTo(1f);
        outgoing.SetActive(false);
        incoming.SetActive(true);
        onCovered?.Invoke();
        yield return FadeTo(0f);
        from.interactable = to.interactable = true;
        SetTransitioning(false);
    }

    private void OnDestroy()
    {
        if (skipButton != null) Destroy(skipButton.gameObject);
        if (transitionVideoPlayer != null)
        {
            transitionVideoPlayer.loopPointReached -= OnTransitionVideoFinished;
            transitionVideoPlayer.errorReceived -= OnTransitionError;
        }
    }

    public void CancelarNickname()
    {
        if (IsTransitioning || playingIntro || enteringGameplay || !nicknamePanel.activeSelf)
            return;
        if (SceneFade.TryLoad("Splash")) SetTransitioning(true);
    }

    public void ConfirmarNickname()
    {
        if (IsTransitioning || playingIntro || enteringGameplay || !nicknamePanel.activeSelf)
            return;
        string nickname = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("Digite um nickname válido.");
            return;
        }

        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.Save();

        playingIntro = true;
        introFinished = false;
        SetTransitioning(true);
        StartCoroutine(PlayIntro());
        Debug.Log("Nickname salvo: " + nickname);
    }

    private IEnumerator PlayIntro()
    {
        yield return FadeTo(1f);
        nicknamePanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        if (transitionVideoPlayer != null && transitionPanel != null && (!enteringGameplay || gameplayIntroClip != null))
        {
            transitionPanel.SetActive(true);
            skipButton.Show(true);
            transitionVideoPlayer.Stop();
            // Preparar sob a cobertura evita revelar a RenderTexture antiga/vazia.
            transitionVideoPlayer.Prepare();
            float deadline = Time.realtimeSinceStartup + 10f;
            while (!transitionVideoPlayer.isPrepared && !introFinished && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!introFinished && transitionVideoPlayer.isPrepared)
            {
                transitionVideoPlayer.Play();
                yield return FadeTo(0f);
                float playbackDeadline = Time.realtimeSinceStartup + Mathf.Max(15f, (float)transitionVideoPlayer.length + 10f);
                while (!introFinished && Time.realtimeSinceStartup < playbackDeadline) yield return null;
                skipButton.Show(false);
                // Mantém o último frame até a cobertura ficar opaca.
                yield return FadeTo(1f);
            }
            else if (!introFinished)
                Debug.LogWarning("A preparação do vídeo excedeu 10 segundos. Continuando o fluxo.");
            transitionVideoPlayer.Stop();
        }
        skipButton.Show(false);
        if (transitionPanel != null) transitionPanel.SetActive(false);
        if (enteringGameplay)
        {
            playingIntro = false;
            if (SceneFade.TryLoad("Beach")) yield break;
            enteringGameplay = false;
        }
        mainMenuPanel.SetActive(true);
        yield return FadeTo(0f);
        playingIntro = false;
        SetTransitioning(false);
    }

    private void OnTransitionVideoFinished(VideoPlayer videoPlayer)
    {
        if (playingIntro)
            introFinished = true;
    }

    public void SkipIntro()
    {
        if (!playingIntro || introFinished || transitionPanel == null || !transitionPanel.activeSelf) return;
        introFinished = true;
        skipButton.Show(false);
    }

    private void OnTransitionError(VideoPlayer videoPlayer, string message)
    {
        if (!playingIntro) return;
        // Se o codec falhar, o jogador ainda consegue acessar o menu.
        Debug.LogWarning("Não foi possível reproduzir a transição: " + message);
        introFinished = true;
    }

    public void PlayGame()
    {
        if (IsTransitioning || enteringGameplay || playingIntro || !mainMenuPanel.activeSelf)
            return;
        enteringGameplay = true;
        playingIntro = true;
        introFinished = false;
        SetTransitioning(true);
        if (GameManager.Instance != null) GameManager.Instance.ResetMission();
        if (transitionVideoPlayer != null && gameplayIntroClip != null)
        {
            transitionVideoPlayer.Stop();
            transitionVideoPlayer.source = VideoSource.VideoClip;
            transitionVideoPlayer.clip = gameplayIntroClip;
            transitionVideoPlayer.isLooping = false;
        }
        StartCoroutine(PlayIntro());
    }

    public void QuitGame()
    {
        if (IsTransitioning) return;
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    public void OpenCredits()
    {
        if (IsTransitioning || playingIntro || enteringGameplay || !mainMenuPanel.activeSelf)
            return;
        if (SceneFade.TryLoad("Credits")) SetTransitioning(true);
    }
}
