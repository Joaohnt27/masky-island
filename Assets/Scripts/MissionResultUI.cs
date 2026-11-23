using TMPro;
using UnityEngine;

public class MissionResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private bool cinematicEnding;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text resultMessage;
    private GameManager mission;

    private void Awake()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    private void Start() => Bind();
    private void OnEnable() => Bind();

    private void Bind()
    {
        if (mission != null)
            mission.OnMissionStateChanged -= Refresh;
        mission = GameManager.Instance;
        if (mission != null)
            mission.OnMissionStateChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        if (resultPanel == null)
            return;
        bool ended = !cinematicEnding && mission != null && mission.IsGameWon();
        resultPanel.SetActive(ended);
        if (ended && resultText != null)
            resultText.text = "A LUZ VOLTOU";
        if (ended && resultMessage != null)
            resultMessage.text = "O farol atravessa a névoa. Ao longe, um barco responde ao sinal.\n\nVocê não voltou por acaso. Voltou para terminar o que começou.\n\nCom a luz restaurada e suas lembranças de volta, a ilha finalmente permite sua partida.";
    }

    public void Restart() => mission?.RestartGame();
    public void MainMenu() => mission?.ReturnToMainMenu();

    private void OnDisable()
    {
        if (mission != null)
            mission.OnMissionStateChanged -= Refresh;
    }
}
