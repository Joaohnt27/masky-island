using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Controla somente a UI/input local; o estado de pausa pertence ao GameManager.
[DefaultExecutionOrder(-200)]
public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private SettingsMenuUI settingsUI;
    private GameManager mission;
    private CrystalInventoryUI inventory;
    private IslandMapUI map;
    private QuestJournalUI journal;
    private KeyCraftingUI crafting;
    private LighthouseGate gate;
    private LighthouseInteraction lighthouse;

    private void Awake() => pausePanel.SetActive(false);

    private void Start()
    {
        mission = GameManager.Instance;
        inventory = FindAnyObjectByType<CrystalInventoryUI>();
        map = FindAnyObjectByType<IslandMapUI>();
        journal = FindAnyObjectByType<QuestJournalUI>();
        crafting = FindAnyObjectByType<KeyCraftingUI>();
        gate = FindAnyObjectByType<LighthouseGate>();
        lighthouse = FindAnyObjectByType<LighthouseInteraction>();
        if (mission != null) mission.OnMissionStateChanged += Refresh;
        Refresh();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            HandleEscape();
    }

    public void HandleEscape()
    {
        if (SceneFade.IsRunning || mission == null || mission.IsGameWon()) return;
        if (settingsUI != null && settingsUI.IsOpen)
        {
            settingsUI.HandleEscape();
            return;
        }
        if (lighthouse != null && lighthouse.IsOpen && !mission.IsPaused) { lighthouse.Close(); return; }
        if (gate != null && gate.IsOpen && !mission.IsPaused) { gate.Close(); return; }
        if (crafting != null && crafting.IsOpen && !mission.IsPaused) { crafting.Close(); return; }
        if (journal != null && journal.IsOpen) { journal.Close(); return; }
        if (map != null && map.IsOpen) { map.Close(); return; }
        if (inventory != null && inventory.IsOpen) { inventory.Close(); return; }
        mission.SetPaused(!mission.IsPaused);
    }

    public void Resume()
    {
        if (!SceneFade.IsRunning) mission?.SetPaused(false);
    }

    public void OpenSettings()
    {
        if (mission != null && mission.IsPaused) settingsUI.Open();
    }

    public void MainMenu() => mission?.ReturnToMainMenu();

    private void Refresh()
    {
        bool paused = mission != null && mission.IsPaused && GameManager.IsGameplayScene;
        if (!paused && settingsUI != null) settingsUI.HideForGameplayState();
        pausePanel.SetActive(paused && (settingsUI == null || !settingsUI.IsOpen));
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDestroy()
    {
        if (mission != null) mission.OnMissionStateChanged -= Refresh;
    }
}


