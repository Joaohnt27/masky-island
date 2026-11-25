using UnityEngine;

/// <summary>
/// Presença narrativa local. O GameManager guarda apenas os encontros já vistos.
/// Não captura cliques, não altera itens e não controla o resultado dos puzzles.
/// </summary>
public class StormShadow : MonoBehaviour
{
    public enum Setting { Beach, Cave, Lighthouse }
    [SerializeField] private Setting setting;
    [SerializeField] private CanvasGroup apparition;
    [SerializeField] private RectTransform figure;
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private StoryLocation location;
    [SerializeField] private LighthouseInteraction lighthouse;
    [SerializeField, Min(1f)] private float duration = 6f;
    private Vector2 origin;
    private float elapsed, arrivalDelay, visibility;
    private bool playing, completed, heardStorm;
    public bool IsAppearing => visibility > .01f;
    public float Visibility => visibility;
    public float Elapsed => elapsed;

    private void Awake()
    {
        if (figure != null) origin = figure.anchoredPosition;
        if (apparition != null)
        {
            apparition.alpha = 0;
            apparition.interactable = apparition.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        var mission = GameManager.Instance;
        if (mission == null || apparition == null || figure == null) return;
        if (mission.IsPaused) return;
        if (SceneFade.IsRunning) { apparition.alpha = 0; return; }

        if (setting == Setting.Lighthouse)
        {
            bool active = lighthouse != null && lighthouse.IsOpen && lighthouse.IsRestoring;
            if (active && !heardStorm)
            {
                GameAudio.Play(GameAudio.Cue.Storm, .45f);
                heardStorm = true;
            }
            if (!active) heardStorm = false;
            // Cada ajuste retira uma camada de sombra; a vitória a dissolve.
            float strength = active && !mission.IsGameWon() ? 1f - lighthouse.QteStep / 3f : 0f;
            visibility = Mathf.MoveTowards(visibility, strength * .85f, Time.deltaTime * 1.5f);
            elapsed += Time.deltaTime;
        }
        else
        {
            var encounter = setting == Setting.Beach ? GameManager.ShadowEncounter.Beach : GameManager.ShadowEncounter.Cave;
            if (completed) return;
            // Espera o jogador recuperar o controle: nunca disputa a introdução ou o altar.
            bool free = player != null && player.IsMovementEnabled &&
                (location == null || !location.HasFocusPanel) && !mission.BlocksGameplay;
            if (!playing)
            {
                if (mission.HasSeenShadow(encounter)) { completed = true; return; }
                bool ready = setting == Setting.Beach ? mission.HasSeenBeachIntroduction : mission.IsCaveCacheOpen;
                if (!ready || !free) { arrivalDelay = 0; return; }
                arrivalDelay += Time.deltaTime;
                if (arrivalDelay < 1f || !mission.TryMarkShadowSeen(encounter)) return;
                playing = true;
                GameAudio.Play(GameAudio.Cue.Storm, .45f);
            }
            if (!free) { apparition.alpha = 0; return; }
            elapsed += Time.deltaTime;
            visibility = Mathf.Min(Mathf.Clamp01(elapsed), Mathf.Clamp01((duration - elapsed) / 1.5f)) * .8f;
            if (elapsed >= duration) { visibility = 0; completed = true; }
        }
        apparition.alpha = visibility;
        figure.anchoredPosition = origin + new Vector2(Mathf.Sin(elapsed * .7f) * 5f, Mathf.Sin(elapsed * 1.4f) * 8f);
        figure.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(elapsed) * 2f);
    }

    private void OnDisable()
    {
        if (apparition != null) apparition.alpha = 0;
        visibility = 0;
    }
}
