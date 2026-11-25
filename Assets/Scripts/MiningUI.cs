using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Controlador fora do painel: detecta a área e apresenta somente a etapa atual.
public class MiningUI : MonoBehaviour
{
    [SerializeField] private MiningSpot miningSpot;
    [SerializeField] private bool altarMode;
    [SerializeField] private GameObject miningPanel;
    [SerializeField] private Button mineButton;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button[] crystalButtons;
    [SerializeField] private Button[] socketButtons;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private GameObject areaHint;

    [Header("Apresentação por etapas")]
    [SerializeField] private TMP_Text stepTitle;
    [SerializeField] private TMP_Text instructions;
    [SerializeField] private TMP_Text primaryLabel;
    [SerializeField] private TMP_Text progressLabel;
    [SerializeField] private TMP_Text closeLabel;
    [SerializeField] private GameObject crystalGroup;
    [SerializeField] private GameObject actionGroup;
    [SerializeField] private GameObject socketGroup;
    [SerializeField] private GameObject completionGroup;
    [SerializeField] private Image[] stepMarkers;
    [SerializeField] private Image[] selectionBorders;
    [SerializeField] private CrystalSymbolGraphic[] socketSymbols;
    [Header("Mineração no cenário")]
    [SerializeField] private GameObject worldMiningPanel;
    [SerializeField] private TMP_Text worldMiningText;
    [SerializeField] private Slider worldProgress;
    [SerializeField] private Button worldCancel;

    private bool dismissed;
    private string feedback;
    private bool feedbackIsError;
    private int observedCrystalCount = -1;
    private static readonly Color gold = new Color(0.96f, 0.77f, 0.40f);
    private static readonly Color muted = new Color(0.19f, 0.28f, 0.31f);
    private static readonly Color green = new Color(0.40f, 0.80f, 0.66f);
    private static readonly Color textColor = new Color(0.79f, 0.85f, 0.84f);

    public void SelectTriangle() => Select(GameManager.CrystalId.Triangle);
    public void SelectCircle() => Select(GameManager.CrystalId.Circle);
    public void SelectSquare() => Select(GameManager.CrystalId.Square);
    public void PlaceFirst() => Place(0);
    public void PlaceSecond() => Place(1);
    public void PlaceThird() => Place(2);

    private bool PanelCanInteract => isActiveAndEnabled && !dismissed && miningPanel != null &&
        miningPanel.activeInHierarchy && miningSpot != null && miningSpot.CanInteract;

    private static int SocketFor(GameManager.CrystalId crystal) =>
        crystal == GameManager.CrystalId.Triangle ? 1 : crystal == GameManager.CrystalId.Circle ? 0 : 2;

    private void Select(GameManager.CrystalId crystal)
    {
        if (worldMiningPanel != null && GameManager.Instance != null && GameManager.Instance.CrystalCount < 3) return;
        if (!PanelCanInteract || GameManager.Instance.IsCrystalSocketFilled(SocketFor(crystal)) ||
            (GameManager.Instance.CrystalCount < 3 && GameManager.Instance.HasCrystal(crystal)) ||
            !miningSpot.SelectCrystal(crystal)) return;
        feedback = null;
        feedbackIsError = false;
        Refresh();
    }

    private void Place(int slot)
    {
        if (!PanelCanInteract) return;
        GameManager mission = GameManager.Instance;
        if (mission.CrystalCount < 3)
        {
            feedback = "Colete os três cristais primeiro. Depois, os encaixes serão liberados.";
            feedbackIsError = true;
        }
        else if (mission.IsCrystalSocketFilled(slot)) return;
        else
        {
            bool placed = mission.TryPlaceCrystal(slot, miningSpot.SelectedCrystal);
            if (!placed) GameAudio.Play(GameAudio.Cue.Error);
            feedbackIsError = !placed;
            feedback = placed ? "Encaixe correto! Continue com o próximo cristal."
                : "Não é esse encaixe. Leia a pista e tente novamente; seu cristal está guardado.";
            if (placed) SelectNextCrystal(mission);
        }
        Refresh();
    }

    // Após cada coleta/encaixe, aponta um item que ainda precisa de atenção.
    private void SelectNextCrystal(GameManager mission)
    {
        if (!miningSpot.CanInteract) return;
        for (int i = 0; i < 3; i++)
        {
            var crystal = (GameManager.CrystalId)i;
            bool pending = mission.CrystalCount < 3 ? !mission.HasCrystal(crystal)
                : !mission.IsCrystalSocketFilled(SocketFor(crystal));
            if (pending) { miningSpot.SelectCrystal(crystal); return; }
        }
    }

    public void ClosePanel()
    {
        if (GameManager.Instance != null && GameManager.Instance.BlocksGameplay) return;
        miningSpot?.CancelMining();
        dismissed = true;
        if (miningPanel != null) miningPanel.SetActive(false);
    }

    public void OpenSockets()
    {
        if (miningSpot == null || !miningSpot.IsPlayerInside || GameManager.Instance == null ||
            GameManager.Instance.CrystalCount < 3 || GameManager.Instance.BlocksGameplay) return;
        dismissed = false;
        Refresh();
    }

    private void Awake()
    {
        if (miningPanel != null)
            miningPanel.GetComponent<AltarPuzzleLayout>()?.InitializePlacedCrystals();
        if (miningPanel != null) miningPanel.SetActive(false);
        if (progressBar == null) return;
        progressBar.minValue = 0;
        progressBar.maxValue = 1;
        progressBar.wholeNumbers = false;
        progressBar.interactable = false;
    }

    private void OnEnable()
    {
        if (mineButton != null) mineButton.onClick.AddListener(StartMining);
        if (worldCancel != null) worldCancel.onClick.AddListener(CancelWorldMining);
    }

    private void CancelWorldMining() => miningSpot?.CancelMining();

    private void StartMining()
    {
        if (!PanelCanInteract) return;
        if (!miningSpot.CanMine) SelectNextCrystal(GameManager.Instance);
        if (!miningSpot.CanMine) return;
        feedback = null;
        feedbackIsError = false;
        miningSpot.StartMining();
        Refresh();
    }

    private void Update() => Refresh();

    private void Refresh()
    {
        if (miningPanel == null) return;
        GameManager mission = GameManager.Instance;
        bool inside = miningSpot != null && miningSpot.IsPlayerInside;
        bool unlocked = mission != null && mission.HasMemory(GameManager.MemoryId.LighthouseKeeper);
        bool solved = mission != null && mission.HasMemory(GameManager.MemoryId.StormNight);
        int count = mission != null ? mission.CrystalCount : 0;
        bool collecting = count < 3;
        if (mission == null || !mission.HasMissionStarted) { feedback = null; observedCrystalCount = -1; }
        if (!inside) { dismissed = false; feedback = null; }
        if (worldMiningPanel != null)
        {
            // Orientações ficam no caderno; este painel só acompanha a ação de minerar.
            bool showWorld = inside && collecting && miningSpot.IsMining;
            worldMiningPanel.SetActive(showWorld);
            if (worldMiningText != null) worldMiningText.text = !unlocked
                ? "Resolva primeiro o bilhete da casa."
                : miningSpot.IsPreparing ? "Guardando o lampião..."
                : miningSpot.IsMining ? $"Minerando a rocha... {Mathf.RoundToInt(miningSpot.Progress * 100)}%"
                : string.IsNullOrEmpty(miningSpot.Result) ? "Clique na rocha para procurar cristais. Nem toda tentativa encontra um."
                : miningSpot.Result + "\nClique na rocha para continuar.";
            if (worldProgress != null) { worldProgress.gameObject.SetActive(miningSpot.IsMining); worldProgress.value = miningSpot.Progress; }
            if (worldCancel != null) { worldCancel.gameObject.SetActive(miningSpot.IsMining); worldCancel.interactable = mission != null && !mission.BlocksGameplay; }
            if (collecting)
            {
                miningPanel.SetActive(false);
                if (areaHint != null) areaHint.SetActive(false);
                return;
            }
        }
        bool visible = inside && !dismissed && (!solved || !mission.HasCollectedKeyMaterials);
        if (areaHint != null) areaHint.SetActive(false);
        miningPanel.SetActive(visible);
        if (!visible) return;

        if (observedCrystalCount != count)
        {
            if (observedCrystalCount >= 0 && count > observedCrystalCount)
                feedback = collecting ? "Cristal guardado! O próximo veio já está selecionado."
                    : "Você encontrou os três! Agora relacione cada forma à pista de um encaixe.";
            feedbackIsError = false;
            observedCrystalCount = count;
            if (unlocked && !solved) SelectNextCrystal(mission);
        }

        int step = solved ? 2 : collecting ? 0 : 1;
        if (crystalGroup != null) crystalGroup.SetActive(!solved);
        if (actionGroup != null) actionGroup.SetActive(collecting && !solved);
        if (socketGroup != null) socketGroup.SetActive(!collecting && !solved);
        if (completionGroup != null) completionGroup.SetActive(solved);
        if (stepMarkers != null)
            for (int i = 0; i < stepMarkers.Length; i++)
                if (stepMarkers[i] != null) stepMarkers[i].color = i < step ? green : i == step && unlocked ? gold : muted;

        string selected = MiningSpot.CrystalName(miningSpot.SelectedCrystal);
        if (stepTitle != null) stepTitle.text = solved ? "O esconderijo se abriu" : !unlocked ? "Comece pelo pergaminho"
            : collecting ? "Colete os três cristais" : "Encontre o encaixe certo";
        if (instructions != null) instructions.text = solved ? "Feche este painel e recolha os materiais junto ao altar."
            : !unlocked ? "Leia a inscrição sobre a mesa da casa. Resolva os símbolos para liberar os veios."
            : miningSpot.IsMining ? "Extraindo o cristal... aguarde a barra completar."
            : collecting ? "Escolha um cristal abaixo ou use o veio já selecionado."
            : altarMode ? $"{selected} selecionado. Clique na mesma forma no altar." : $"{selected} selecionado. Clique no encaixe cuja pista combina com essa forma.";
        if (primaryLabel != null) primaryLabel.text = !unlocked ? "RESOLVA O PERGAMINHO"
            : miningSpot.IsMining ? "EXTRAINDO..." : $"MINERAR {selected}";
        if (mineButton != null) mineButton.interactable = collecting && miningSpot.CanInteract;
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(miningSpot.IsMining);
            progressBar.value = miningSpot.Progress;
        }
        if (progressLabel != null) progressLabel.text = miningSpot.IsMining
            ? $"{Mathf.RoundToInt(miningSpot.Progress * 100)}%" : "Cada tentativa pode encontrar um cristal. Continue procurando.";
        if (closeLabel != null) closeLabel.text = miningSpot.IsMining ? "CANCELAR E SAIR" : solved ? "RECOLHER MATERIAIS" : "CONTINUAR DEPOIS";
        if (inventoryText != null) inventoryText.text = solved ? $"{mission.RecoveredMemoryCount} DE 3 LEMBRANÇAS RECUPERADAS"
            : collecting ? $"{count} de 3 cristais guardados" : $"{mission.FilledCrystalSocketCount} de 3 encaixes concluídos";
        if (resultText != null)
        {
            resultText.rectTransform.anchoredPosition = new Vector2(0, solved ? -110 : -226);
            resultText.rectTransform.sizeDelta = new Vector2(510, solved ? 100 : 58);
            resultText.fontSize = solved ? 27 : 18;
            resultText.fontStyle = solved ? FontStyles.Italic : FontStyles.Normal;
            resultText.text = solved ? "Na noite da tempestade, o farol se apagou."
                : !unlocked ? "O estranho de capuz vermelho pode ajudar você a começar."
                : miningSpot.IsMining ? "" : feedback ?? (collecting ? "Os encaixes aparecem depois que você coletar os três cristais."
                    : "Se errar, tente outro encaixe. Você não perde o cristal.");
            resultText.color = feedbackIsError && !solved ? new Color(1f, 0.62f, 0.49f) : textColor;
        }
        RefreshCards(mission, unlocked, collecting);
    }

    private void RefreshCards(GameManager mission, bool unlocked, bool collecting)
    {
        for (int i = 0; crystalButtons != null && i < crystalButtons.Length; i++)
        {
            if (crystalButtons[i] == null) continue;
            var crystal = (GameManager.CrystalId)i;
            bool owned = mission != null && mission.HasCrystal(crystal);
            bool placed = mission != null && mission.IsCrystalSocketFilled(SocketFor(crystal));
            bool selected = unlocked && !placed && miningSpot.SelectedCrystal == crystal;
            crystalButtons[i].interactable = miningSpot.CanInteract && !placed && (!collecting || !owned);
            var label = crystalButtons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = MiningSpot.CrystalName(crystal) + "\n<size=16>" +
                (!unlocked ? "BLOQUEADO" : placed ? "ENCAIXADO" : collecting && owned ? "GUARDADO" : selected ? "SELECIONADO" : "SELECIONAR") + "</size>";
            if (selectionBorders != null && i < selectionBorders.Length && selectionBorders[i] != null)
                selectionBorders[i].color = placed || (collecting && owned) ? green : selected ? gold : muted;
        }
        for (int i = 0; socketButtons != null && i < socketButtons.Length; i++)
        {
            if (socketButtons[i] == null) continue;
            bool filled = mission != null && mission.IsCrystalSocketFilled(i);
            socketButtons[i].interactable = !collecting && miningSpot.CanInteract && !filled;
            var label = socketButtons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = filled ? "ENCAIXADO" : i == 0 ? "Sem cantos" : i == 1 ? "Três pontas" : "Quatro lados";
            if (socketSymbols != null && i < socketSymbols.Length && socketSymbols[i] != null)
                socketSymbols[i].gameObject.SetActive(filled);
        }
    }

    private void OnDisable()
    {
        if (mineButton != null) mineButton.onClick.RemoveListener(StartMining);
        if (worldCancel != null) worldCancel.onClick.RemoveListener(CancelWorldMining);
        if (worldMiningPanel != null) worldMiningPanel.SetActive(false);
        miningSpot?.CancelMining();
        if (miningPanel != null) miningPanel.SetActive(false);
        if (areaHint != null) areaHint.SetActive(false);
    }
}

