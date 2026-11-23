using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A bancada apresenta o minigame; somente o GameManager consome os materiais e entrega a chave.
public class KeyCraftingUI : MonoBehaviour
{
    public enum CraftStage { Mold, Metal, Shaping, Ready, Complete }
    [SerializeField] private GameObject panel;
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private TMP_Text instruction, feedback, progress, actionLabel;
    [SerializeField] private Button moldButton, metalButton, actionButton, closeButton;
    [SerializeField] private Image mold, metal, finishedKey;
    [SerializeField] private GameObject timing;
    [SerializeField] private RectTransform cursor, target;
    [SerializeField, Min(.1f)] private float cursorSpeed = .65f;
    [SerializeField, Range(.1f,.4f)] private float targetWidth = .22f;
    private readonly float[] targetCenters = { .30f, .70f, .45f };
    private float elapsed, cooldown, impact;
    private GameManager mission;
    public bool IsOpen { get; private set; }
    public CraftStage Stage { get; private set; }
    public int SuccessfulHits { get; private set; }
    public float CursorValue => Mathf.PingPong(elapsed * cursorSpeed, 1f);
    public float TargetCenter => targetCenters[Mathf.Min(SuccessfulHits, targetCenters.Length-1)];

    private void Awake()
    {
        panel.SetActive(false);
        moldButton.onClick.AddListener(PlaceMold);
        metalButton.onClick.AddListener(PlaceMetal);
        actionButton.onClick.AddListener(Action);
        closeButton.onClick.AddListener(Close);
    }
    private void Start(){mission=GameManager.Instance;if(mission!=null)mission.OnMissionStateChanged+=OnStateChanged;}
    public void Open()
    {
        mission=GameManager.Instance;
        if(IsOpen||mission==null||!mission.CanCraftLighthouseKey||!player.IsMovementEnabled)return;
        Stage=CraftStage.Mold;SuccessfulHits=0;elapsed=cooldown=impact=0;
        IsOpen=true;player.AcquireMovementLock(this);panel.SetActive(true);
        feedback.text="Os materiais só serão usados quando você recolher a chave pronta.";
        GameAudio.Play(GameAudio.Cue.Open);Refresh();
    }
    private bool CanAct => IsOpen && mission!=null && !mission.BlocksGameplay && !SceneFade.IsRunning;
    public void PlaceMold()
    {
        if(!CanAct||Stage!=CraftStage.Mold)return;
        Stage=CraftStage.Metal;feedback.text="Molde apoiado. Agora encaixe a peça de metal.";GameAudio.Play(GameAudio.Cue.Confirm);Refresh();
    }
    public void PlaceMetal()
    {
        if(!CanAct||Stage!=CraftStage.Metal)return;
        Stage=CraftStage.Shaping;elapsed=0;feedback.text="Acerte a faixa dourada. Se errar, tente novamente.";GameAudio.Play(GameAudio.Cue.Confirm);Refresh();
    }
    private void Update()
    {
        if(!CanAct)return;
        if(Stage==CraftStage.Shaping)
        {
            elapsed+=Time.deltaTime;cooldown=Mathf.Max(0,cooldown-Time.deltaTime);
            cursor.anchoredPosition=new Vector2((CursorValue-.5f)*440,0);
            if(Keyboard.current!=null&&Keyboard.current.spaceKey.wasPressedThisFrame)Strike();
        }
        impact=Mathf.MoveTowards(impact,0,Time.deltaTime*4);
        metal.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(impact*Mathf.PI*4)*4);
    }
    public void Strike()
    {
        if(!CanAct||Stage!=CraftStage.Shaping||cooldown>0)return;
        cooldown=.30f;impact=1;
        if(Mathf.Abs(CursorValue-TargetCenter)<=targetWidth*.5f)
        {
            SuccessfulHits++;GameAudio.Play(GameAudio.Cue.Pickaxe);
            feedback.text="Bom golpe! "+SuccessfulHits+" de 3 ajustes concluídos.";
            if(SuccessfulHits==3){Stage=CraftStage.Ready;feedback.text="A chave está pronta! Recolha para guardá-la na mochila.";}
            else elapsed=0;
        }
        else {feedback.text="Fora da marca. Espere o indicador entrar na faixa dourada e tente de novo.";GameAudio.Play(GameAudio.Cue.Error);}
        Refresh();
    }
    public void Action(){if(Stage==CraftStage.Shaping)Strike();else if(Stage==CraftStage.Ready)Collect();else if(Stage==CraftStage.Complete)Close();}
    public void Collect()
    {
        if(!CanAct||Stage!=CraftStage.Ready||!mission.TryCraftLighthouseKey())return;
        Stage=CraftStage.Complete;feedback.text="Chave guardada. A peça de metal e o molde foram utilizados.";GameAudio.Play(GameAudio.Cue.Success);Refresh();
    }
    private void Refresh()
    {
        mold.enabled=Stage!=CraftStage.Mold;
        metal.enabled=Stage==CraftStage.Shaping;
        finishedKey.enabled=Stage==CraftStage.Ready||Stage==CraftStage.Complete;
        moldButton.interactable=Stage==CraftStage.Mold;metalButton.interactable=Stage==CraftStage.Metal;
        actionButton.gameObject.SetActive((int)Stage>=(int)CraftStage.Shaping);timing.SetActive(Stage==CraftStage.Shaping);
        target.sizeDelta=new Vector2(440*targetWidth,28);target.anchoredPosition=new Vector2((TargetCenter-.5f)*440,0);
        instruction.text=Stage==CraftStage.Mold?"1. Coloque o molde na bancada":Stage==CraftStage.Metal?"2. Encaixe a peça de metal":Stage==CraftStage.Shaping?"3. Ajuste a chave com três golpes precisos":Stage==CraftStage.Ready?"4. Recolha a chave pronta":"Chave do farol fabricada";
        progress.text="AJUSTES  "+SuccessfulHits+" / 3";
        actionLabel.text=Stage==CraftStage.Ready?"RECOLHER CHAVE":Stage==CraftStage.Complete?"VOLTAR À CASA":"GOLPEAR  [ESPAÇO]";
    }
    private void OnStateChanged(){if(IsOpen&&mission!=null&&!mission.HasMissionStarted)Close();}
    public void Close()
    {
        if(mission!=null&&mission.IsPaused)return;
        IsOpen=false;if(panel!=null)panel.SetActive(false);if(player!=null)player.ReleaseMovementLock(this);
    }
    private void OnDisable(){IsOpen=false;if(panel!=null)panel.SetActive(false);if(player!=null)player.ReleaseMovementLock(this);}
    private void OnDestroy(){if(mission!=null)mission.OnMissionStateChanged-=OnStateChanged;}
}
