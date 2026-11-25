using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Perseguição local: aviso, aproximação, captura e intervalo. Não altera o inventário.</summary>
public class StormPursuit : MonoBehaviour
{
    public enum Phase { Waiting, Warning, Chasing, Recovering }
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private StoryLocation location;
    [SerializeField] private SpriteRenderer shadow;
    [SerializeField] private TMP_Text message;
    [SerializeField] private CanvasGroup messageGroup;
    [SerializeField, Min(1f)] private float firstDelay = 6f;
    [SerializeField, Min(1f)] private float interval = 20f;
    [SerializeField, Min(1f)] private float warningTime = 2.5f;
    [SerializeField, Min(1f)] private float chaseTime = 9f;
    [SerializeField, Range(.1f, .95f)] private float speedRatio = .7f;
    [SerializeField, Min(.1f)] private float catchDistance = .5f;
    [SerializeField, Min(1f)] private float lanternReach = 3f;
    private Vector3 safePoint, shadowPoint;
    private float remaining, animationTime, grace;
    private bool initialized, suspended;
    public Phase CurrentPhase { get; private set; }
    public Vector3 SafePoint => safePoint;
    public Vector3 ShadowPoint => shadowPoint;
    public float Remaining => remaining;
    public int CaptureCount { get; private set; }

    private IEnumerator Start()
    {
        Hide();
        // StoryLocation posiciona o jogador conforme a porta de chegada no próprio Start.
        yield return null;
        if (player == null || shadow == null) { enabled = false; yield break; }
        safePoint = player.transform.position;
        remaining = firstDelay;
        initialized = true;
    }

    private void Update()
    {
        var gm = GameManager.Instance;
        if (!initialized || gm == null) return;
        bool enabledByStory = gm.HasMemory(GameManager.MemoryId.LighthouseKeeper) && gm.IsMissionActive() && !gm.IsGameWon();
        if (!enabledByStory)
        {
            CurrentPhase = Phase.Waiting; remaining = firstDelay; CaptureCount = 0; Hide(); return;
        }
        // Nunca captura enquanto os controles estão indisponíveis (mineração, diálogo ou menus).
        if (gm.IsPaused || SceneFade.IsRunning || !player.IsMovementEnabled ||
            (location != null && location.HasFocusPanel))
        {
            suspended = true; Hide(); return;
        }
        if (suspended) { grace = 1.5f; suspended = false; }
        animationTime += Time.deltaTime;
        if (grace > 0) { grace -= Time.deltaTime; Draw(); return; }
        if (gm.HasLantern && gm.IsLanternLit && Vector2.Distance(player.transform.position,shadowPoint) <= lanternReach &&
            (CurrentPhase == Phase.Warning || CurrentPhase == Phase.Chasing))
        {
            Retreat("A luz do lampião afastou a Sombra!"); return;
        }
        remaining -= Time.deltaTime;
        switch (CurrentPhase)
        {
            case Phase.Waiting:
                if (remaining <= 0 && TryChooseSpawn())
                {
                    CurrentPhase = Phase.Warning; remaining = warningTime;
                    SetMessage(gm.HasLantern ? "A Sombra se aproxima! Use E para acender o lampião." : "A Sombra se aproxima! Fuja e procure o lampião na casa.");
                    GameAudio.Play(GameAudio.Cue.Storm, .8f);
                }
                break;
            case Phase.Warning:
                if (remaining <= 0)
                {
                    CurrentPhase = Phase.Chasing; remaining = chaseTime;
                    SetMessage(gm.HasLantern ? "Acenda o lampião com E para afastar a Sombra!" : "Não deixe a Sombra tocar você! Afaste-se com W A S D.");
                }
                break;
            case Phase.Chasing:
                shadowPoint = Vector3.MoveTowards(shadowPoint, player.transform.position,
                    player.MoveSpeed * speedRatio * Time.deltaTime);
                if (gm.HasLantern && gm.IsLanternLit && Vector2.Distance(shadowPoint,player.transform.position) <= lanternReach)
                    Retreat("A luz do lampião afastou a Sombra!");
                else if (Vector2.Distance(shadowPoint, player.transform.position) <= catchDistance)
                {
                    player.ReturnToSafePoint(safePoint); CaptureCount++;
                    GameAudio.Play(GameAudio.Cue.Error, .6f);
                    Retreat("A Sombra pegou você! Você voltou à entrada, com seus itens. Procure o lampião!");
                }
                else if (remaining <= 0) Retreat("Você escapou! A névoa se desfez por enquanto.");
                break;
            case Phase.Recovering:
                if (remaining <= 0) { CurrentPhase = Phase.Waiting; remaining = interval; Hide(); }
                break;
        }
        Draw();
    }

    private bool TryChooseSpawn()
    {
        // A névoa pode atravessar obstáculos; nasce sempre longe do jogador.
        float best = 0;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI / 4;
            Vector3 point = player.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 4;
            point.x = Mathf.Clamp(point.x, player.WalkMin.x, player.WalkMax.x);
            point.y = Mathf.Clamp(point.y, player.WalkMin.y, player.WalkMax.y);
            float distance = Vector2.Distance(point, player.transform.position);
            if (distance >= 3f && distance > best)
            { best = distance; shadowPoint = point; }
        }
        if (best == 0) remaining = 2; // Nunca forçar um spawn em cima do personagem.
        return best > 0;
    }
    private void Retreat(string text)
    {
        CurrentPhase = Phase.Recovering; remaining = 3f; SetMessage(text); Draw();
    }
    private void SetMessage(string text) { if (message != null) message.text = text; }
    private void Draw()
    {
        if (messageGroup != null) messageGroup.alpha = CurrentPhase == Phase.Waiting ? 0 : 1;
        shadow.enabled = CurrentPhase == Phase.Warning || CurrentPhase == Phase.Chasing;
        if (!shadow.enabled) return;
        shadow.transform.position = shadowPoint + new Vector3(0, .55f + Mathf.Sin(animationTime * 2) * .08f, -.1f);
        shadow.flipX = player.transform.position.x > shadowPoint.x;
        shadow.color = new Color(1,1,1,CurrentPhase == Phase.Warning ? .65f : 1f);
    }
    private void Hide()
    {
        if (shadow != null) shadow.enabled = false;
        if (messageGroup != null) messageGroup.alpha = 0;
    }
    private void OnDisable() => Hide();
}
