using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject dialoguePanel;

    [Header("Textos")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text npcNameText;

    [Header("Nome do NPC")]
    [SerializeField] private string npcName = "Estranho";

    [SerializeField] private PlayerClickMovement playerMovement;
    public bool IsOpen { get; private set; }
    [Header("Introdução do personagem (praia)")]
    [SerializeField] private bool playerIntroduction;
    [SerializeField] private TMP_Text advanceLabel;
    [SerializeField] private UnityEngine.UI.Image speakerPortrait;
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private Sprite npcPortrait;

    private void ShowPortrait(bool playerSpeaking)
    {
        if (speakerPortrait == null) return;
        speakerPortrait.sprite = playerSpeaking ? playerPortrait : npcPortrait;
        speakerPortrait.enabled = speakerPortrait.sprite != null;
    }
    private readonly string[] introductionLines =
    {
        "Eita... o que estou fazendo aqui?",
        "O que aconteceu comigo? Eu não lembro de nada...",
        "O que será que aconteceu? Espera... o que é aquela casa no horizonte?",
        "Vou até ela para ver se alguém pode me ajudar.",
        "Olha, tem um livro ali na areia. Vou pegar para ver o que tem."
    };

    private string[] dialogueLines =
    {
        "Você acordou na praia? Então a ilha trouxe mais alguém de volta.",
        "Aqui, as lembranças se perdem. Mas deixam pistas.",
        "O farol está apagado. Sem sua luz, nenhum barco encontra a saída.",
        "Três lembranças podem restaurar sua luz. A primeira está escondida em uma inscrição.",
        "Volte à casa e clique no bilhete sobre a mesa para ler.",
        "Ele conta como o céu muda. Toque nos símbolos seguindo a ordem da pista.",
        "Antes de procurar os cristais, volte à casa perto da praia. Deixei uma mochila minha ao lado da cama; pode pegá-la para guardar o que encontrar.",
        "Anote no seu diário: buscar a mochila, depois decifrar o bilhete. Abra a mochila com I quando estiver com ela.",
        "Para minerar, pegue a picareta que deixei logo na entrada da caverna, à esquerda, encostada nas pedras. Depois, aproxime-se dos veios e clique para usar a ferramenta.",
        "Leve este mapa da ilha. Pressione M para abri-lo. Você pode viajar pelo mapa para os lugares que já visitou."
    };

    private readonly string[] reminderLines =
    {
        "A primeira lembrança está no bilhete da casa. Leia a inscrição com atenção.",
        "O que se despede primeiro? O que desperta depois? E o que mostra o caminho?"
    };
    private readonly string[] backpackReminderLines = {
        "A mochila está na casa perto da praia, ao lado da cama. Pode pegá-la: você vai precisar dela para guardar os cristais.",
        "Depois, examine o bilhete sobre a mesa para encontrar a primeira lembrança."
    };
    private readonly string[] recoveredLines =
    {
        "Você cuidava da luz... Então o farol já fez parte da sua vida.",
        "Siga o caminho à direita até a caverna. A picareta está logo na entrada, à esquerda, encostada nas pedras. Pegue-a antes de clicar nos veios para minerar.",
        "Nem toda tentativa encontra um cristal. Confira os encontrados com I e use as três formas nos encaixes indicados pelas pistas."
    };
    private readonly string[] stormLines =
    {
        "Na noite da tempestade, o farol se apagou... Agora sabemos quando tudo mudou.",
        "Os cristais abriram um esconderijo junto ao altar. Recolha a peça de metal e o molde que estão lá.",
        "Leve os materiais à bancada da casa. Você poderá fabricar a chave do farol."
    };
    private readonly string[] returnLines =
    {
        "Você voltou para terminar o que começou. Agora as três lembranças fazem sentido.",
        "Você era o guardião do farol e retornou para restaurar sua luz.",
        "Siga o caminho ao fundo do centro até o farol. Use a chave e observe a inscrição para descobrir a senha do cadeado.",
        "Lá dentro, ajuste o mecanismo: espere o indicador entrar na faixa dourada e pressione Espaço, ou clique no botão, para acender a luz."
    };
    private string[] currentDialogue;
    private int currentLine = 0;
    private bool personalThought;
    private System.Action thoughtFinished;

    public bool ShowThought(string[] lines, System.Action finished = null)
    {
        if (IsOpen || SceneFade.IsRunning || lines == null || lines.Length == 0 ||
            (playerMovement != null && !playerMovement.IsMovementEnabled)) return false;
        personalThought = true;
        ShowPortrait(true);
        thoughtFinished = finished;
        currentDialogue = lines;
        currentLine = 0;
        IsOpen = true;
        playerMovement?.AcquireMovementLock(this);
        string name = PlayerPrefs.GetString("PlayerNickname", "").Trim();
        npcNameText.richText = false;
        npcNameText.text = string.IsNullOrEmpty(name) ? "Você" : name;
        dialoguePanel.SetActive(true);
        ShowCurrentLine();
        return true;
    }

    private IEnumerator Start()
    {
        dialoguePanel.SetActive(false);
        if (!playerIntroduction) yield break;
        while (SceneFade.IsRunning || (GameManager.Instance != null && GameManager.Instance.IsPaused)) yield return null;
        var mission = GameManager.Instance;
        if (mission != null && !mission.HasSeenBeachIntroduction && !mission.HasVisitedHouse && !mission.HasMissionStarted)
            StartDialogue();
    }

    private void Update()
    {
        // A pausa cobre a introdução sem perder a fala nem liberar o movimento.
        if (playerIntroduction && IsOpen && GameManager.Instance != null)
            dialoguePanel.SetActive(!GameManager.Instance.IsPaused);
    }

    public void StartDialogue()
    {
        if (IsOpen || SceneFade.IsRunning || (playerIntroduction && GameManager.Instance != null && GameManager.Instance.HasSeenBeachIntroduction) || (playerMovement != null && !playerMovement.IsMovementEnabled))
            return;
        IsOpen = true;
        playerMovement?.AcquireMovementLock(this);
        currentLine = 0;
        GameManager mission = GameManager.Instance;
        currentDialogue = playerIntroduction ? introductionLines : mission != null && mission.HasMissionStarted && !mission.HasBackpack
            ? backpackReminderLines : mission != null && mission.HasMemory(GameManager.MemoryId.Return)
            ? returnLines : mission != null && mission.HasMemory(GameManager.MemoryId.StormNight)
            ? stormLines : mission != null && mission.HasMemory(GameManager.MemoryId.LighthouseKeeper)
            ? recoveredLines
            : mission != null && mission.HasMissionStarted ? reminderLines : dialogueLines;

        if (playerIntroduction)
        {
            string chosenName = PlayerPrefs.GetString("PlayerNickname", "").Trim();
            npcNameText.richText = false;
            npcNameText.text = string.IsNullOrEmpty(chosenName) ? "Você" : chosenName;
        }
        else npcNameText.text = npcName;

        ShowPortrait(playerIntroduction);

        dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (!IsOpen || (GameManager.Instance != null && GameManager.Instance.BlocksGameplay))
            return;
        currentLine++;

        if (currentLine >= currentDialogue.Length)
        {
            CloseDialogue();
            if (personalThought)
            {
                personalThought = false;
                var finished = thoughtFinished;
                thoughtFinished = null;
                finished?.Invoke();
                return;
            }

            if (playerIntroduction)
            {
                GameManager.Instance?.CompleteBeachIntroduction();
                return;
            }

            // Começa a missão depois do diálogo
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartMission();
                if (currentDialogue == dialogueLines) GameManager.Instance.ReceiveMapFromNpc();
                var mission = GameManager.Instance;
                if (mission.HasJournal && !mission.HasLearnedJournal)
                    ShowThought(new[] {
                        "São muitas pistas... vou organizar tudo no diário!",
                        "Vou anotar o que preciso fazer e riscar o que já terminei. Assim não esqueço meu caminho."
                    }, mission.LearnJournal);
            }

            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        dialogueText.text = currentDialogue[currentLine];
        if (advanceLabel != null) advanceLabel.text = currentLine == currentDialogue.Length - 1
            ? "CONTINUAR A EXPLORAR  ›" : "CONTINUAR  ›";
    }

    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        IsOpen = false;
        playerMovement?.ReleaseMovementLock(this);
    }

    private void OnDisable()
    {
        IsOpen = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        playerMovement?.ReleaseMovementLock(this);
    }
}
