using TMPro;
using UnityEngine;
using UnityEngine.UI;

// O pergaminho existente cuida de abrir/fechar e bloquear o movimento.
// Este componente cuida apenas da sequência e do feedback do puzzle.
public class SymbolSequencePuzzle : MonoBehaviour
{
    public enum Symbol { Sun, Moon, Star, Boat, Fish, Lighthouse }

    [Header("Sequência e lembrança")]
    [SerializeField] private Symbol[] solution = { Symbol.Sun, Symbol.Moon, Symbol.Star };
    [SerializeField] private GameManager.MemoryId memory = GameManager.MemoryId.LighthouseKeeper;
    [SerializeField, TextArea] private string memoryText = "Você cuidava da luz.";
    [SerializeField] private bool requiresPreviousMemory;
    [SerializeField] private GameManager.MemoryId previousMemory = GameManager.MemoryId.StormNight;
    [SerializeField, TextArea] private string lockedText = "Recupere a lembrança dos cristais antes de decifrar este bilhete.";
    [SerializeField, TextArea] private string nextStepText;
    [SerializeField] private TMP_Text clueText;
    [SerializeField, TextArea] private string openingClue;
    [SerializeField, TextArea] private string unlockedClue;

    [Header("UI do pergaminho")]
    [SerializeField] private Button sunButton;
    [SerializeField] private Button moonButton;
    [SerializeField] private Button starButton;
    [SerializeField] private TMP_Text feedbackText;

    private GameManager mission;
    private int enteredCount;
    private string feedback;
    public int EnteredCount => enteredCount;
    public bool IsSolved => mission != null && mission.HasMemory(memory);
    private bool IsUnlocked => mission != null && (!requiresPreviousMemory || mission.HasMemory(previousMemory));

    private void OnEnable()
    {
        enteredCount = 0;
        feedback = "Toque nos símbolos na ordem indicada pela inscrição.";
        Bind();
    }

    private void Start() => Bind();

    private void Bind()
    {
        if (mission != null) mission.OnMissionStateChanged -= Refresh;
        mission = GameManager.Instance;
        if (mission != null) mission.OnMissionStateChanged += Refresh;
        Refresh();
    }

    public void SelectSun() => Select(Symbol.Sun);
    public void SelectMoon() => Select(Symbol.Moon);
    public void SelectStar() => Select(Symbol.Star);
    public void SelectBoat() => Select(Symbol.Boat);
    public void SelectFish() => Select(Symbol.Fish);
    public void SelectLighthouse() => Select(Symbol.Lighthouse);

    private void Select(Symbol symbol)
    {
        if (!isActiveAndEnabled || mission == null || !mission.IsMissionActive() ||
            mission.BlocksGameplay || SceneFade.IsRunning || IsSolved || !IsUnlocked ||
            solution == null || solution.Length == 0)
            return;

        if (symbol != solution[enteredCount])
        {
            GameAudio.Play(GameAudio.Cue.Error);
            enteredCount = 0;
            feedback = "A inscrição não reagiu. A sequência recomeçou. Tente outra ordem.";
        }
        else
        {
            enteredCount++;
            if (enteredCount < solution.Length) GameAudio.Play(GameAudio.Cue.Confirm);
            feedback = $"A inscrição reagiu: {enteredCount} de {solution.Length} símbolos.";
            if (enteredCount == solution.Length)
            {
                // O GameManager impede recompensas repetidas, inclusive após trocar de cena.
                mission.TryRecoverMemory(memory);
                enteredCount = 0;
            }
        }
        Refresh();
    }

    private void Refresh()
    {
        if (clueText != null) clueText.text = IsUnlocked ? unlockedClue : openingClue;
        bool started = mission != null && mission.HasMissionStarted;
        if (!started)
        {
            enteredCount = 0;
            feedback = "Toque nos símbolos na ordem indicada pela inscrição.";
        }
        bool canSelect = mission != null && mission.IsMissionActive() &&
            !mission.BlocksGameplay && !SceneFade.IsRunning && IsUnlocked && !IsSolved && solution != null && solution.Length > 0;
        if (sunButton != null) sunButton.interactable = canSelect;
        if (moonButton != null) moonButton.interactable = canSelect;
        if (starButton != null) starButton.interactable = canSelect;
        if (feedbackText != null)
        {
            feedbackText.text = IsSolved
                ? $"LEMBRANÇA RECUPERADA\n{memoryText}\n\nLembranças: {mission.RecoveredMemoryCount} / 3" + (string.IsNullOrEmpty(nextStepText) ? "" : "\n" + nextStepText)
                : !started
                    ? "Antes de decifrar a inscrição, converse com o estranho de capuz vermelho."
                    : !IsUnlocked ? lockedText : feedback;
        }
    }

    private void OnDisable()
    {
        if (mission != null) mission.OnMissionStateChanged -= Refresh;
        enteredCount = 0;
    }
}
