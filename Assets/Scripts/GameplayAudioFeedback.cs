using UnityEngine;

public class GameplayAudioFeedback : MonoBehaviour
{
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private MiningSpot mining;
    private GameManager mission;
    private int memories, crystals, sockets, strike = -1;
    private bool won;
    private Vector3 lastPosition;
    private float distance;
    private void Start()
    {
        mission = GameManager.Instance;
        lastPosition = player.transform.position;
        if (mission != null) { Remember(); mission.OnMissionStateChanged += Changed; }
    }
    private void Remember()
    {
        memories = mission.RecoveredMemoryCount; crystals = mission.CrystalCount;
        sockets = mission.FilledCrystalSocketCount; won = mission.IsGameWon();
    }
    private void Changed()
    {
        if (!won && mission.IsGameWon()) GameAudio.Play(GameAudio.Cue.Beacon);
        else if (mission.RecoveredMemoryCount > memories) GameAudio.Play(GameAudio.Cue.Success);
        else if (mission.CrystalCount > crystals) GameAudio.Play(GameAudio.Cue.Crystal);
        else if (mission.FilledCrystalSocketCount > sockets) GameAudio.Play(GameAudio.Cue.Confirm);
        Remember();
    }
    private void Update()
    {
        if (player == null) return;
        float delta = Vector3.Distance(lastPosition, player.transform.position);
        lastPosition = player.transform.position;
        if (mission == null || mission.BlocksGameplay || SceneFade.IsRunning) { distance = 0; return; }
        if (player.IsMovementEnabled && delta < 1)
        {
            distance += delta;
            if (distance >= .55f) { distance = 0; GameAudio.Play(GameAudio.Cue.Footstep); }
        }
        else distance = 0;
        if (mining != null && mining.IsMining && !mining.IsPreparing)
        {
            int next = mining.ImpactIndex;
            if (next >= 0 && next != strike) { strike = next; GameAudio.Play(GameAudio.Cue.Pickaxe); }
        }
        else strike = -1;
    }
    private void OnDestroy() { if (mission != null) mission.OnMissionStateChanged -= Changed; }
}
