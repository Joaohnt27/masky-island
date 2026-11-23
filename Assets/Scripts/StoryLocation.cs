using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Dados locais de chegada e HUD; progresso continua no GameManager.
public class StoryLocation : MonoBehaviour
{
    [System.Serializable] public struct Arrival { public string from; public Vector3 position; }
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private TMP_Text objective;
    [SerializeField] private bool hideObjectiveHeader;
    [SerializeField] private GameObject hover;
    [SerializeField] private GameObject[] focusPanels;
    [SerializeField] private Arrival[] arrivals;
    private GameManager mission;

    private void Start()
    {
        foreach (var arrival in arrivals)
            if (arrival.from == SceneFade.OriginScene) { player.transform.position = arrival.position; break; }
        mission = GameManager.Instance;
        if (mission != null) mission.OnMissionStateChanged += Refresh;
        Refresh();
    }
    private void Refresh()
    {
        if (mission == null || objective == null) return;
        string scene = SceneManager.GetActiveScene().name;
        string location = scene == "Beach" ? "PRAIA" : scene == "House" ? "CASA" : scene == "Island" ? "CENTRO" : scene == "Cave" ? "CAVERNA" : "FAROL";
        string task = !mission.HasMissionStarted
            ? scene == "Beach" && !mission.HasVisitedHouse ? "Você não lembra quem é. Siga o caminho até a casa."
            : !mission.HasReadHouseNotice ? "Procure um aviso sobre a mesa da casa." : "Encontre o trabalhador junto ao poço, no centro da ilha."
            : !mission.HasBackpack ? "Volte à casa e pegue a mochila ao lado da cama."
            : !mission.HasMemory(GameManager.MemoryId.LighthouseKeeper) ? "Leia o bilhete na mesa da casa e resolva os símbolos do céu."
            : !mission.HasPickaxe ? "Pegue a picareta à esquerda da entrada da caverna."
            : !mission.HasMemory(GameManager.MemoryId.StormNight) ? mission.CrystalCount < 3
                ? $"Minere os veios da caverna. Cristais no inventário: {mission.CrystalCount} / 3."
                : "Na caverna, coloque os três cristais nos encaixes."
            : !mission.HasCollectedKeyMaterials ? "Recolha os materiais no esconderijo aberto junto ao altar da caverna."
            : !mission.HasLighthouseKey ? "Use a bancada da casa para fabricar a chave do farol."
            : !mission.IsLighthouseUnlocked ? "No exterior do farol, examine a inscrição, use a chave e decifre o cadeado." : "Entre no farol e acenda a luz usando o mecanismo.";
        objective.text = $"<color=#F5C566>{location}</color>  •  {mission.RecoveredMemoryCount}/3 lembranças\n{task}";
    }
    public bool HasFocusPanel
    {
        get {
            if (focusPanels != null) foreach (var panel in focusPanels)
                if (panel != null && panel.activeInHierarchy) return true;
            return false;
        }
    }
    public string ObjectiveText => objective != null ? objective.text : "";
    private void LateUpdate()
    {
        bool focused = HasFocusPanel;
        if (objective != null) objective.transform.parent.gameObject.SetActive(!focused && !hideObjectiveHeader);
        if (hover != null) hover.SetActive(!focused);
    }
    private void OnDestroy() { if (mission != null) mission.OnMissionStateChanged -= Refresh; }
}

