using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

// Vídeo final; o encerramento ilustrado permanece como alternativa se o vídeo falhar.
public class EndingCutscene : MonoBehaviour
{
    [SerializeField] private RawImage picture;
    [SerializeField] private Texture[] shots;
    [SerializeField] private TMP_Text caption;
    [SerializeField] private CanvasGroup shade;
    [SerializeField] private Graphic lightBeam;
    [SerializeField, Min(1)] private float shotDuration = 5f;
    private bool leaving;
    [SerializeField] private TMP_FontAsset skipButtonFont;
    private CutsceneSkipButton skipButton;
    [SerializeField] private VideoClip finalGameplayClip;
    private VideoPlayer finalVideo;
    private bool videoFinished, videoFailed;
    private static readonly string[] lines = {
        "A luz atravessou a névoa. Pela primeira vez, você lembrou quem era.",
        "Você era o guardião do farol. E voltou para terminar o que começou.",
        "Com o caminho iluminado, a ilha deixou de ser uma prisão. Era hora de voltar para casa."
    };
    private IEnumerator Start()
    {
        skipButton = CutsceneSkipButton.Create(transform, skipButtonFont, Finish);
        if (finalGameplayClip != null)
        {
            picture.texture = Texture2D.blackTexture;
            caption.transform.parent.gameObject.SetActive(false);
            lightBeam.gameObject.SetActive(false);
            shade.alpha = 0;
        }
        while (SceneFade.IsRunning) yield return null;
        skipButton.Show(true);
        if (finalGameplayClip != null)
        {
            yield return PlayFinalVideo();
            if (leaving) yield break;
            if (!videoFailed) { Finish(); yield break; }
            caption.transform.parent.gameObject.SetActive(true);
        }
        for (int i = 0; i < lines.Length && !leaving; i++)
        {
            picture.texture = shots[Mathf.Min(i, shots.Length - 1)];
            caption.text = lines[i];
            lightBeam.gameObject.SetActive(i == 0);
            for (float t = 0; t < shotDuration && !leaving; t += Time.unscaledDeltaTime)
            {
                float progress = t / shotDuration;
                picture.rectTransform.localScale = Vector3.one * Mathf.Lerp(1, 1.07f, progress);
                shade.alpha = Mathf.Clamp01(Mathf.Max(1 - t, 1 - (shotDuration - t)));
                if (i == 0) lightBeam.color = new Color(1, .85f, .45f, Mathf.Min(.32f, t * .15f));
                yield return null;
            }
        }
        Finish();
    }
    private IEnumerator PlayFinalVideo()
    {
        finalVideo = gameObject.AddComponent<VideoPlayer>();
        finalVideo.playOnAwake = false;
        finalVideo.isLooping = false;
        finalVideo.source = VideoSource.VideoClip;
        finalVideo.clip = finalGameplayClip;
        finalVideo.renderMode = VideoRenderMode.APIOnly;
        finalVideo.audioOutputMode = VideoAudioOutputMode.Direct;
        finalVideo.controlledAudioTrackCount = finalGameplayClip.audioTrackCount;
        gameObject.AddComponent<SettingsAudioChannel>().SetCategory(SettingsManager.AudioCategory.Music);
        finalVideo.loopPointReached += OnVideoFinished;
        finalVideo.errorReceived += OnVideoError;
        finalVideo.Prepare();
        float deadline = Time.realtimeSinceStartup + 15;
        while (!finalVideo.isPrepared && !videoFailed && !leaving && Time.realtimeSinceStartup < deadline) yield return null;
        if (leaving) yield break;
        if (!finalVideo.isPrepared || videoFailed) { videoFailed = true; finalVideo.Stop(); yield break; }
        var fit = picture.GetComponent<AspectRatioFitter>();
        if (fit == null) fit = picture.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fit.aspectRatio = (float)finalVideo.width / Mathf.Max(1f, finalVideo.height);
        finalVideo.Play();
        deadline = Time.realtimeSinceStartup + Mathf.Max(30f, (float)finalVideo.length + 15f);
        while (!videoFinished && !videoFailed && !leaving && Time.realtimeSinceStartup < deadline)
        {
            if (finalVideo.texture != null) picture.texture = finalVideo.texture;
            yield return null;
        }
        if (!videoFinished && !leaving) videoFailed = true;
        if (videoFailed) { finalVideo.Stop(); fit.enabled = false; }
    }
    private void OnVideoFinished(VideoPlayer video) => videoFinished = true;
    private void OnVideoError(VideoPlayer video, string message)
    {
        videoFailed = true;
        Debug.LogWarning("Vídeo final indisponível; usando encerramento ilustrado: " + message);
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Finish();
    }
    private void OnDestroy() { if (skipButton != null) Destroy(skipButton.gameObject); }
    public void Finish()
    {
        if (!leaving && SceneFade.TryLoad("Credits"))
        {
            leaving = true;
            if (finalVideo != null) finalVideo.Stop();
            if (skipButton != null) skipButton.Show(false);
        }
    }
}
