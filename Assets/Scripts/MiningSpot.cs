using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MiningSpot : MonoBehaviour
{
    [Header("Área (distância 3D, sem Rigidbody)")]
    [SerializeField] private PlayerClickMovement player;
    [SerializeField, Min(0.1f)] private float interactionDistance = 2f;
    [SerializeField, Min(0.1f)] private float miningDuration = 4f;
    [Header("Descoberta por tentativa concluída")]
    [SerializeField, Range(0f, 1f)] private float crystalChance = 0.35f;
    [SerializeField] private Transform approachPoint;
    [SerializeField] private Collider socketCollider;
    public bool IsSocket(Collider collider) => socketCollider != null && collider == socketCollider;
    [System.Serializable] public struct RockTarget
    {
        public Collider collider;
        public Transform approach;
    }
    [SerializeField] private RockTarget[] rockTargets;
    [SerializeField] private Transform socketApproach;
    [SerializeField, Min(.1f)] private float miningReach = .25f;
    private Transform selectedApproach;
    private Collider selectedRock;
    public const int StrokeCount = 6;
    public float StrokeTime => Progress * StrokeCount;
    public int ImpactIndex => Mathf.FloorToInt(StrokeTime - .5f);
    public bool FacesLeft => selectedRock != null && Mathf.Abs(selectedRock.bounds.center.x - ApproachPosition.x) > .05f
        ? selectedRock.bounds.center.x < ApproachPosition.x : ApproachPosition.x < 0;
    public Vector3 ApproachPosition => selectedApproach != null ? selectedApproach.position :
        approachPoint != null ? approachPoint.position : transform.position;

    public bool SelectTarget(Collider clicked)
    {
        if (IsMining) return false;
        if (clicked == null) { selectedApproach = approachPoint; selectedRock = null; return true; }
        if (IsSocket(clicked)) { selectedApproach = socketApproach != null ? socketApproach : approachPoint; selectedRock = null; return true; }
        if (rockTargets != null) foreach (var target in rockTargets)
            if (target.collider == clicked && target.approach != null)
            { selectedApproach = target.approach; selectedRock = clicked; return true; }
        return false;
    }
    public bool CanDiscover => isActiveAndEnabled && GameManager.Instance != null &&
        GameManager.Instance.CanWorkOnCrystals && GameManager.Instance.CrystalCount < 3;

    // O jogador descobre o próximo cristal na rocha, sem escolher a recompensa na UI.
    public void StartDiscovery()
    {
        if (!CanInteract || !CanDiscover) return;
        for (int i = 0; i < 3; i++)
            if (!GameManager.Instance.HasCrystal((GameManager.CrystalId)i))
            {
                SelectedCrystal = (GameManager.CrystalId)i;
                StartMining();
                return;
            }
    }

    [Header("Animação opcional: conectar quando houver clips")]
    [SerializeField] private UnityEvent onMiningStarted = new UnityEvent();
    [SerializeField] private UnityEvent onMiningFinished = new UnityEvent();

    // Evento de conclusão para efeitos opcionais; a coleta é validada pelo GameManager.
    [SerializeField] private UnityEvent onMiningCompleted = new UnityEvent();

    private Coroutine miningRoutine;
    private GameManager mission;
    public bool IsMining { get; private set; }
    public bool IsPreparing { get; private set; }
    public float Progress { get; private set; }
    public string Result { get; private set; } = "";
    public GameManager.CrystalId SelectedCrystal { get; private set; }
    public bool CanInteract => IsPlayerInside && player.IsMovementEnabled &&
        GameManager.Instance != null && GameManager.Instance.CanWorkOnCrystals && !IsMining;

    public bool SelectCrystal(GameManager.CrystalId crystal)
    {
        if (!CanInteract || !System.Enum.IsDefined(typeof(GameManager.CrystalId), crystal)) return false;
        SelectedCrystal = crystal;
        Result = "";
        return true;
    }

    public static string CrystalName(GameManager.CrystalId crystal)
    {
        switch (crystal)
        {
            case GameManager.CrystalId.Triangle: return "TRIÂNGULO";
            case GameManager.CrystalId.Circle: return "CÍRCULO";
            default: return "QUADRADO";
        }
    }

    public bool IsPlayerInside => isActiveAndEnabled && player != null &&
        Vector3.Distance(transform.position, player.transform.position) <= interactionDistance;

    private bool MissionAllowsMining => GameManager.Instance != null &&
        GameManager.Instance.IsMissionActive() && GameManager.Instance.HasBackpack && GameManager.Instance.HasPickaxe &&
        GameManager.Instance.HasMemory(GameManager.MemoryId.LighthouseKeeper) &&
        !GameManager.Instance.HasMemory(GameManager.MemoryId.StormNight);

    public bool CanMine => CanInteract && !GameManager.Instance.HasCrystal(SelectedCrystal) &&
        Vector3.Distance(player.transform.position, ApproachPosition) <= miningReach;

    private void Start() => Bind();
    private void OnEnable() => Bind();

    private void Bind()
    {
        if (mission != null)
            mission.OnMissionStateChanged -= OnMissionChanged;
        mission = GameManager.Instance;
        if (mission != null)
            mission.OnMissionStateChanged += OnMissionChanged;
    }

    private void OnMissionChanged()
    {
        if (!MissionAllowsMining)
            CancelMining();
        if (mission != null && !mission.HasMissionStarted)
        {
            SelectedCrystal = GameManager.CrystalId.Triangle;
            Result = "";
            Progress = 0f;
        }
    }

    public void StartMining()
    {
        if (!CanMine)
            return;
        IsMining = true;
        IsPreparing = true;
        Progress = 0f;
        Result = "Procurando cristais na rocha...";
        player.AcquireMovementLock(this);
        miningRoutine = StartCoroutine(Mine());
    }

    private IEnumerator Mine()
    {
        GameManager.CrystalId crystal = SelectedCrystal;
        var lantern = FindAnyObjectByType<PlayerLantern>();
        if (lantern != null) yield return lantern.StowForMining();
        IsPreparing = false;
        onMiningStarted.Invoke();
        float elapsed = 0f;
        while (elapsed < miningDuration)
        {
            yield return null;
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                continue;
            if (!IsPlayerInside || !MissionAllowsMining ||
                Vector3.Distance(player.transform.position, ApproachPosition) > miningReach)
            {
                miningRoutine = null;
                FinishAttempt();
                Progress = 0f;
                Result = "Mineração cancelada.";
                yield break;
            }
            elapsed += Time.deltaTime;
            Progress = Mathf.Clamp01(elapsed / miningDuration);
        }

        miningRoutine = null;
        FinishAttempt();
        if (crystalChance <= 0f || (crystalChance < 1f && Random.value >= crystalChance))
        {
            Result = "Nenhum cristal desta vez. Tente minerar novamente.";
            GameAudio.Play(GameAudio.Cue.Error, .5f);
            yield break;
        }
        if (GameManager.Instance != null && GameManager.Instance.TryCollectCrystal(crystal))
        {
            Result = $"Cristal {CrystalName(crystal)} adicionado ao inventário!";
            onMiningCompleted.Invoke();
        }
    }

    public void CancelMining()
    {
        if (!IsMining)
            return;
        if (miningRoutine != null)
            StopCoroutine(miningRoutine);
        miningRoutine = null;
        FinishAttempt();
        Progress = 0f;
        Result = "Mineração cancelada.";
    }

    private void FinishAttempt()
    {
        IsMining = false;
        IsPreparing = false;
        if (player != null)
            player.ReleaseMovementLock(this);
        onMiningFinished.Invoke();
    }

    private void OnDisable()
    {
        CancelMining();
        if (mission != null)
            mission.OnMissionStateChanged -= OnMissionChanged;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
