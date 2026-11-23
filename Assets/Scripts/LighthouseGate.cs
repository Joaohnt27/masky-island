using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LighthouseGate : Interactable
{
    [SerializeField] private PlayerClickMovement movement;
    [SerializeField] private GameObject panel, worldLock;
    [SerializeField] private TMP_Text clue, feedback, keyStatus;
    [SerializeField] private TMP_Text[] digits;
    [SerializeField] private Button[] increase, decrease;
    [SerializeField] private Button readButton, keyButton, unlockButton, enterButton, closeButton;
    private readonly int[] values = new int[3];
    private GameManager mission;
    public bool IsOpen => panel != null && panel.activeSelf;
    public int Code => values[0]*100 + values[1]*10 + values[2];
    private bool CanAct => IsOpen && mission != null && !mission.BlocksGameplay && !SceneFade.IsRunning;
    protected override void Start()
    {
        base.Start(); panel.SetActive(false); mission=GameManager.Instance;
        if(mission!=null)mission.OnMissionStateChanged+=Refresh;
        for(int i=0;i<3;i++){int slot=i;increase[i].onClick.AddListener(()=>ChangeDigit(slot,1));decrease[i].onClick.AddListener(()=>ChangeDigit(slot,-1));}
        readButton.onClick.AddListener(ReadClue);keyButton.onClick.AddListener(UseKey);
        unlockButton.onClick.AddListener(Submit);enterButton.onClick.AddListener(Enter);closeButton.onClick.AddListener(Close);
        Refresh();
    }
    public override void Interact()
    {
        if(IsOpen||mission==null||mission.BlocksGameplay||SceneFade.IsRunning||!movement.IsMovementEnabled||!IsInReach(movement.transform))return;
        movement.AcquireMovementLock(this);panel.SetActive(true);
        feedback.text=mission.IsLighthouseUnlocked?"O cadeado já está aberto. Pode entrar.":"Uma fechadura e um cadeado protegem a porta. A inscrição ao lado guarda a pista.";
        Refresh();GameAudio.Play(GameAudio.Cue.Open);
    }
    public void ReadClue()
    {
        if(!CanAct)return;
        mission.ReadLighthouseClue();Refresh();feedback.text="Observe a ordem dos sinais. A pista também ficou no seu caderno.";
    }
    public void UseKey()
    {
        if(!CanAct)return;
        if(mission.TryUseLighthouseKey()){feedback.text="A chave girou! Falta soltar o cadeado com a senha.";GameAudio.Play(GameAudio.Cue.Confirm);}
        else feedback.text=mission.HasUsedLighthouseKey?"A fechadura já está aberta.":"Preciso fabricar a chave na bancada da casa.";
        Refresh();
    }
    public void ChangeDigit(int slot,int delta)
    {
        if(!CanAct||mission.IsLighthouseUnlocked||slot<0||slot>=3)return;
        values[slot]=(values[slot]+delta%10+10)%10;Refresh();GameAudio.Play(GameAudio.Cue.Click);
    }
    public void Submit()
    {
        if(!CanAct||mission.IsLighthouseUnlocked)return;
        if(mission.TryUnlockLighthouse(Code)){feedback.text="Clique! O cadeado se soltou. O caminho está livre.";GameAudio.Play(GameAudio.Cue.Success);}
        else {feedback.text=!mission.HasUsedLighthouseKey?"Use a chave do farol na fechadura primeiro.":!mission.HasReadLighthouseClue?"Leia a inscrição junto à porta para descobrir a ordem.":"O cadeado não abriu. Conte apenas os lados retos de cada sinal.";GameAudio.Play(GameAudio.Cue.Error);}
        Refresh();
    }
    public void Enter()
    {
        if(!CanAct||!mission.IsLighthouseUnlocked)return;
        if(SceneFade.TryLoad("Lighthouse"))Close();
    }
    private void Refresh()
    {
        if(mission==null)return;
        worldLock.SetActive(!mission.IsLighthouseUnlocked);
        clue.text=mission.HasReadLighthouseClue?"QUADRADO  ·  CÍRCULO  ·  TRIÂNGULO\n\nNa ordem dos sinais, conte os lados retos.\nQuem não tem lado reto guarda o vazio.\n\nCada sinal revela um algarismo.":"Há uma inscrição na pedra junto à porta.\n\nLeia antes de tentar a combinação.";
        keyStatus.text=mission.HasUsedLighthouseKey?"FECHADURA ABERTA":mission.HasLighthouseKey?"CHAVE NA MOCHILA":"FALTA A CHAVE DA CASA";
        keyButton.interactable=!mission.HasUsedLighthouseKey&&mission.HasLighthouseKey;
        unlockButton.gameObject.SetActive(!mission.IsLighthouseUnlocked);enterButton.gameObject.SetActive(mission.IsLighthouseUnlocked);
        for(int i=0;i<3;i++){digits[i].text=values[i].ToString();increase[i].interactable=decrease[i].interactable=!mission.IsLighthouseUnlocked;}
    }
    public void Close(){if(mission!=null&&mission.IsPaused)return;panel.SetActive(false);movement.ReleaseMovementLock(this);}
    private void OnDisable(){if(panel!=null)panel.SetActive(false);if(movement!=null)movement.ReleaseMovementLock(this);}
    private void OnDestroy(){if(mission!=null)mission.OnMissionStateChanged-=Refresh;}
}
