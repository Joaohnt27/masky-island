using System.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Apenas apresenta o progresso já existente no GameManager.
public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button openButton;
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private StoryLocation story;
    [SerializeField] private TMP_Text heading, missions, notes;
    [SerializeField] private GameObject[] writtenHeadings;
    private GameManager mission;
    private struct Entry { public string text; public bool done; }
    private readonly List<Entry> entries = new List<Entry>();
    private static readonly HashSet<string> inkedTasks = new HashSet<string>();
    private Coroutine writing;
    private int writingTask = -1;
    private float strokeProgress;
    public bool IsWriting => writing != null;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearInk() => inkedTasks.Clear();
    public bool IsOpen { get; private set; }
    private bool Available => mission != null && mission.HasJournal;
    private void Awake(){panel.SetActive(false);openButton.gameObject.SetActive(false);}
    private void Start(){mission=GameManager.Instance;if(mission!=null)mission.OnMissionStateChanged+=Refresh;}
    private void Update()
    {
        if(IsOpen && (!Available || mission.BlocksGameplay || SceneFade.IsRunning))Close();
        if(Keyboard.current!=null && Keyboard.current.jKey.wasPressedThisFrame)Toggle();
        bool visible=Available&&!mission.BlocksGameplay&&!SceneFade.IsRunning&&!IsOpen&&!story.HasFocusPanel;
        openButton.gameObject.SetActive(visible);
        openButton.interactable=visible&&player.IsMovementEnabled;
    }
    public void Toggle()
    {
        if(IsOpen){Close();return;}
        if(!Available||mission.BlocksGameplay||SceneFade.IsRunning||!player.IsMovementEnabled||story.HasFocusPanel)return;
        IsOpen=true;player.AcquireMovementLock(this);panel.SetActive(true);Refresh();GameAudio.Play(GameAudio.Cue.Open);
    }
    public void Close()
    {
        if(writing!=null){StopCoroutine(writing);writing=null;}writingTask=-1;
        bool wasOpen=IsOpen;IsOpen=false;if(panel!=null)panel.SetActive(false);if(player!=null)player.ReleaseMovementLock(this);
        if(wasOpen&&!SceneFade.IsRunning)GameAudio.Play(GameAudio.Cue.Close);
    }
    private void Refresh()
    {
        if(mission==null)return;
        if(!mission.HasJournal)inkedTasks.Clear();
        if(!IsOpen)return;
        string name=PlayerPrefs.GetString("PlayerNickname","").Trim();heading.richText=false;
        heading.text=string.IsNullOrEmpty(name)?"MEU CADERNO":"CADERNO DE "+name;
        if(writtenHeadings!=null)foreach(var item in writtenHeadings)if(item!=null)item.SetActive(mission.HasLearnedJournal);
        if(!mission.HasLearnedJournal){missions.text="";notes.text="";return;}
        bool first=mission.HasMemory(GameManager.MemoryId.LighthouseKeeper),storm=mission.HasMemory(GameManager.MemoryId.StormNight),home=mission.HasMemory(GameManager.MemoryId.Return);
        entries.Clear();
        Add("Procurar ajuda na casa.",mission.HasVisitedHouse);
        if(mission.HasVisitedHouse||mission.HasReadHouseNotice)Add("Ler o aviso sobre a mesa.",mission.HasReadHouseNotice);
        if(mission.HasReadHouseNotice||mission.HasMissionStarted)Add("Falar com o trabalhador no centro.",mission.HasMissionStarted);
        if(mission.HasMissionStarted)Add("Buscar a mochila na casa.",mission.HasBackpack);
        if(mission.HasBackpack)Add("Decifrar os símbolos do bilhete.",first);
        if(first)Add("Buscar o lampião junto à lareira. Usar E.",mission.HasLantern);
        if(first)Add("Pegar a picareta na entrada da caverna.",mission.HasPickaxe);
        if(first&&mission.HasPickaxe)Add("Encontrar os três cristais na caverna.",mission.CrystalCount>=3);
        if(mission.CrystalCount>=3)Add("Usar os cristais nos encaixes do altar.",storm);
        if(storm)Add("Recolher os materiais no esconderijo.",mission.HasCollectedKeyMaterials);
        if(mission.HasCollectedKeyMaterials)Add("Fabricar a chave na bancada da casa.",mission.HasLighthouseKey);
        if(home)Add("Destrancar a porta e o cadeado do farol.",mission.IsLighthouseUnlocked);
        if(mission.IsLighthouseUnlocked)Add("Restaurar a luz do farol.",mission.IsGameWon());
        RenderTasks();
        if(writing==null)writing=StartCoroutine(WriteCompletedTasks());
        var text=new StringBuilder("Encontrei este diário na praia. Minha primeira tarefa: procurar ajuda na casa.");
        if(mission.HasReadHouseNotice)text.Append("\n\nO aviso da casa diz que há alguém trabalhando no poço do centro.");
        if(mission.HasMissionStarted)text.Append("\n\nO estranho me ofereceu uma mochila na casa. Preciso dela para guardar os cristais.");
        if(first)text.Append("\n\nEu cuidava do farol. A picareta fica à esquerda da entrada da caverna; preciso dela para minerar.");
        if(storm)text.Append("\n\nOs cristais abriram um esconderijo na caverna. A peça de metal e o molde servem para fabricar a chave na casa.");
        if(home)text.Append("\n\nEu era o guardião do farol. Voltei para restaurar sua luz.");
        if(mission.HasReadLighthouseClue)text.Append("\n\nInscrição do farol: quadrado, círculo, triângulo. Cada número conta apenas os lados retos. O círculo não tem nenhum.");
        notes.text=text.ToString();
    }
    private void Add(string task,bool done) => entries.Add(new Entry { text=task, done=done });
    private void RenderTasks()
    {
        var text=new StringBuilder();
        for(int i=0;i<entries.Count;i++)
        {
            var entry=entries[i];
            int crossed=entry.done&&inkedTasks.Contains(entries[i].text)?entry.text.Length:
                i==writingTask?Mathf.FloorToInt(entry.text.Length*strokeProgress):0;
            text.Append(entry.done?"<color=#71614A>":"<color=#173F40>");
            if(crossed>0)text.Append("<s>").Append(entry.text.Substring(0,crossed)).Append("</s>");
            text.Append(entry.text.Substring(crossed)).Append("</color>\n\n");
        }
        missions.text=text.ToString();
    }
    private IEnumerator WriteCompletedTasks()
    {
        // Primeiro yield permite guardar a referência da coroutine mesmo sem tarefas pendentes.
        yield return null;
        for(int i=0;i<entries.Count;i++)
        {
            if(!entries[i].done||inkedTasks.Contains(entries[i].text))continue;
            writingTask=i;
            for(float elapsed=0;elapsed<.7f;elapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f))
            {
                strokeProgress=Mathf.Clamp01(elapsed/.7f);RenderTasks();yield return null;
            }
            inkedTasks.Add(entries[i].text);writingTask=-1;RenderTasks();
            yield return new WaitForSecondsRealtime(.12f);
        }
        writingTask=-1;writing=null;
    }
    private void OnDisable()=>Close();
    private void OnDestroy(){if(mission!=null)mission.OnMissionStateChanged-=Refresh;}
}

