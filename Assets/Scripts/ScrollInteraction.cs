using UnityEngine;

public class ScrollInteraction : Interactable
{
    [SerializeField] private GameObject returnPanel;
    [Header("Pergaminho")]
    [SerializeField] private GameObject scrollPanel;
    [SerializeField] private PlayerClickMovement playerMovement;
    [SerializeField] private DialogueController noticeDialogue;
    [SerializeField] private KeyCraftingUI craftingUI;

    protected override void Start()
    {
        base.Start();
        
        scrollPanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);
    }

    public override void Interact()
    {
        if (playerMovement != null && !playerMovement.IsMovementEnabled)
            return;
        var mission = GameManager.Instance;
        if (noticeDialogue != null && mission != null && (!mission.HasReadHouseNotice || !mission.HasMissionStarted))
        {
            noticeDialogue.ShowThought(new[] {
                "Tem um aviso sobre a mesa: ‘Precisa de ajuda? Estou trabalhando no poço, no centro da ilha.’",
                "Então há alguém por perto! Vou até o centro falar com esse trabalhador. Melhor anotar no diário."
            }, mission.ReadHouseNotice);
            return;
        }
        if (mission != null && mission.HasMemory(GameManager.MemoryId.StormNight) && noticeDialogue != null)
        {
            if (mission.HasLighthouseKey) noticeDialogue.ShowThought(new[]{"A chave do farol está pronta e guardada na mochila."});
            else if (!mission.HasCollectedKeyMaterials) noticeDialogue.ShowThought(new[]{"Preciso recolher os materiais no esconderijo que os cristais abriram na caverna."});
            else if (craftingUI != null) craftingUI.Open();
            return;
        }
        playerMovement?.AcquireMovementLock(this);
        bool returning = returnPanel != null && GameManager.Instance != null && GameManager.Instance.HasMemory(GameManager.MemoryId.StormNight);
        (returning ? returnPanel : scrollPanel).SetActive(true);
    }

    public void CloseScroll()
    {
        if (GameManager.Instance != null && GameManager.Instance.BlocksGameplay) return;
        scrollPanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);
        playerMovement?.ReleaseMovementLock(this);
    }

    private void OnDisable()
    {
        if (scrollPanel != null)
            scrollPanel.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);
        playerMovement?.ReleaseMovementLock(this);
    }
}
