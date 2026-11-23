using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LighthouseInteraction : Interactable
{
    [SerializeField] private PlayerClickMovement playerMovement;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button activateButton, closeButton;
    [SerializeField] private GameObject beacon;
    [SerializeField, Min(2f)] private float stepWindow = 8f;
    [SerializeField] private Slider qteProgress;
    [Header("Ajustes de precisão")]
    [SerializeField] private Button strikeButton;
    [SerializeField] private RectTransform cursor, target;
    [SerializeField] private GameObject timing;
    [SerializeField] private TMP_Text feedbackText, timeText, helpText;
    [SerializeField] private TMP_Text[] stageLabels;
    private static readonly string[] stages = { "Alinhar as lentes", "Regular as engrenagens", "Acender a luz" };
    private static readonly float[] centers = { .30f, .70f, .45f };
    private Coroutine restoration;
    private int step;
    private float remaining, elapsed, cooldown;
    private bool finishing;
    private string feedback;
    public bool IsOpen => panel != null && panel.activeSelf;
    public bool IsRestoring => restoration != null;
    public int QteStep => step;
    public float Remaining => remaining;
    public float CursorValue => Mathf.PingPong(elapsed * (.6f + step * .12f), 1f);
    public float TargetCenter => centers[Mathf.Min(step,2)];
    public float TargetWidth => .24f - Mathf.Min(step,2) * .03f;
    private GameManager Mission => GameManager.Instance;
    private bool Ready => Mission != null && Mission.IsMissionActive() && Mission.HasAllMemories && Mission.IsLighthouseUnlocked;
    private bool CanAct => IsOpen && Ready && !Mission.BlocksGameplay && !SceneFade.IsRunning && !finishing;

    protected override void Start()
    {
        base.Start(); panel.SetActive(false);
        if(strikeButton!=null)strikeButton.onClick.AddListener(SubmitPulse);
        Refresh();
    }
    protected override void Update()
    {
        base.Update();
        if(IsOpen&&!finishing&&(Mission==null||SceneFade.IsRunning||!Mission.IsMissionActive()))CancelAndClose();
        if(IsRestoring&&CanAct&&Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)SubmitPulse();
        Refresh();
    }
    public override void Interact()
    {
        if(playerMovement==null||!playerMovement.IsMovementEnabled||IsOpen||Mission==null||Mission.BlocksGameplay||SceneFade.IsRunning)return;
        playerMovement.AcquireMovementLock(this);step=0;finishing=false;feedback="Prepare o mecanismo. São três ajustes para devolver a luz à ilha.";panel.SetActive(true);Refresh();
    }
    public void Activate()
    {
        if(!isActiveAndEnabled||!CanAct||IsRestoring)return;
        step=0;elapsed=cooldown=0;remaining=stepWindow;
        feedback="Espere o indicador entrar na faixa dourada e confirme o ajuste.";
        restoration=StartCoroutine(Restore());Refresh();
    }
    public void SubmitQteKey(Key key){if(key==Key.Space)SubmitPulse();}
    public void SubmitPulse()
    {
        if(!CanAct||!IsRestoring||step>=3||cooldown>0)return;
        cooldown=.35f;
        if(Mathf.Abs(CursorValue-TargetCenter)>TargetWidth*.5f)
        {
            feedback="Ainda não! Confirme dentro da faixa dourada. Os ajustes feitos estão seguros.";
            GameAudio.Play(GameAudio.Cue.Error);Refresh();return;
        }
        step++;elapsed=0;remaining=stepWindow;GameAudio.Play(GameAudio.Cue.Confirm);
        feedback=step==3?"A luz voltou! A ilha tem um caminho outra vez.":"Ajuste concluído! Prepare-se para a próxima etapa.";
        Refresh();
    }
    private IEnumerator Restore()
    {
        while(step<3)
        {
            yield return null;
            if(!Ready||SceneFade.IsRunning){CancelAndClose();yield break;}
            if(Mission.IsPaused)continue;
            elapsed+=Time.deltaTime;cooldown=Mathf.Max(0,cooldown-Time.deltaTime);remaining-=Time.deltaTime;
            if(remaining<=0){remaining=stepWindow;elapsed=0;cooldown=.35f;feedback="O mecanismo perdeu o ritmo. Tente esta etapa novamente.";GameAudio.Play(GameAudio.Cue.Error);}
        }
        while(Mission!=null&&Mission.IsPaused)yield return null;
        if(!Ready||SceneFade.IsRunning){CancelAndClose();yield break;}
        finishing=true;beacon.SetActive(true);Mission.CompleteMission();GameAudio.Play(GameAudio.Cue.Beacon);Refresh();
        yield return new WaitForSecondsRealtime(1.25f);
        panel.SetActive(false);restoration=null;playerMovement.ReleaseMovementLock(this);SceneFade.TryLoad("Ending");
    }
    public void Close(){if(finishing||(Mission!=null&&Mission.BlocksGameplay))return;CancelAndClose();}
    private void Refresh()
    {
        if(!IsRestoring)beacon.SetActive(Mission!=null&&Mission.IsGameWon());
        if(!IsOpen)return;
        bool running=IsRestoring&&step<3;
        activateButton.gameObject.SetActive(!IsRestoring);activateButton.interactable=CanAct;
        closeButton.interactable=Mission!=null&&!Mission.BlocksGameplay&&!finishing;
        if(strikeButton!=null){strikeButton.gameObject.SetActive(running);strikeButton.interactable=CanAct&&cooldown<=0;}
        if(timing!=null)timing.SetActive(running);
        if(cursor!=null)cursor.anchoredPosition=new Vector2((CursorValue-.5f)*440,0);
        if(target!=null){target.anchoredPosition=new Vector2((TargetCenter-.5f)*440,0);target.sizeDelta=new Vector2(440*TargetWidth,28);}
        if(qteProgress!=null){qteProgress.gameObject.SetActive(running);qteProgress.value=remaining/stepWindow;}
        if(timeText!=null)timeText.text=running?$"TEMPO DO AJUSTE  {Mathf.CeilToInt(remaining)} s":"";
        statusText.text=step==3?"FAROL RESTAURADO":!Ready?"Fabrique a chave e destranque a porta do farol.":IsRestoring?$"{step+1}. {stages[step]}":"Restaure o mecanismo do farol";
        if(feedbackText!=null)feedbackText.text=feedback;
        if(helpText!=null)helpText.text=step==3?"A luz atravessa a névoa. É hora de lembrar.":"Clique em AJUSTAR ou pressione Espaço\nquando o indicador entrar na faixa dourada.";
        for(int i=0;stageLabels!=null&&i<stageLabels.Length;i++)
        {
            stageLabels[i].text=$"{i+1}. {stages[i]}"+(i<step?"\nCONCLUÍDO":"");
            stageLabels[i].color=i<step?new Color(.16f,.43f,.31f):i==step?new Color(.65f,.38f,.1f):new Color(.32f,.36f,.32f);
        }
    }
    private void CancelAndClose()
    {
        if(restoration!=null)StopCoroutine(restoration);restoration=null;finishing=false;
        if(panel!=null)panel.SetActive(false);if(playerMovement!=null)playerMovement.ReleaseMovementLock(this);
        if(beacon!=null)beacon.SetActive(Mission!=null&&Mission.IsGameWon());
    }
    private void OnDisable()=>CancelAndClose();
}


