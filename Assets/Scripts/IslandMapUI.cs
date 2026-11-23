using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IslandMapUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private PlayerClickMovement player;
    [SerializeField] private StoryLocation story;
    [SerializeField] private Button openButton, travelButton;
    [SerializeField] private TMP_Text objective, detailTitle, detailText, travelLabel;
    [SerializeField] private RawImage preview;
    [SerializeField] private TMP_Text controls;
    [SerializeField] private Texture[] previews;
    [SerializeField] private Image[] pins;
    [SerializeField] private TMP_Text[] pinLabels;
    private GameManager mission;
    private string selected;
    private static string MapScene(string scene) => scene == "Lighthouse" ? "LighthouseExterior" : scene;
    public bool IsOpen { get; private set; }
    public string SelectedLocation => selected;
    public bool CanTravel => IsOpen && mission != null && mission.HasMap && !mission.BlocksGameplay && !SceneFade.IsRunning &&
        mission.HasVisitedLocation(selected) && selected != MapScene(SceneManager.GetActiveScene().name) && GameManager.IsExplorationScene(selected);
    public static string LocationName(string scene) => scene == "Beach" ? "Praia" : scene == "House" ? "Casa" : scene == "Island" ? "Centro" : scene == "Cave" ? "Caverna" : "Farol";

    private void Awake() { panel.SetActive(false); openButton.gameObject.SetActive(false); }
    private void Start()
    {
        mission=GameManager.Instance;
        if(mission!=null)mission.OnMissionStateChanged+=Refresh;
        Refresh();
    }
    private void Update()
    {
        if(IsOpen && (mission==null || !mission.HasMap || mission.BlocksGameplay || SceneFade.IsRunning)) Close();
        if(Keyboard.current!=null && Keyboard.current.mKey.wasPressedThisFrame) Toggle();
        bool available=mission!=null && mission.HasMap && !mission.BlocksGameplay && !SceneFade.IsRunning && player.IsMovementEnabled && !story.HasFocusPanel;
        openButton.gameObject.SetActive(mission!=null && mission.HasMap && !IsOpen && !story.HasFocusPanel);
        openButton.interactable=available;
        if(IsOpen) fade.alpha=Mathf.MoveTowards(fade.alpha,1,Time.unscaledDeltaTime*8);
    }
    public void Toggle()
    {
        if(IsOpen){Close();return;}
        if(mission==null || !mission.HasMap || mission.BlocksGameplay || SceneFade.IsRunning || !player.IsMovementEnabled || story.HasFocusPanel) return;
        IsOpen=true; player.AcquireMovementLock(this); panel.SetActive(true);fade.alpha=0;
        selected=MapScene(SceneManager.GetActiveScene().name);Refresh();GameAudio.Play(GameAudio.Cue.Open);
    }
    public void SelectLocation(string scene)
    {
        scene=MapScene(scene);
        if(!IsOpen || !GameManager.IsExplorationScene(scene))return;
        selected=scene;Refresh();
    }
    public void Travel()
    {
        if(!CanTravel)return;
        if(SceneFade.TryLoad(selected)) Close();
    }
    public void Close()
    {
        bool wasOpen=IsOpen;IsOpen=false;
        if(panel!=null)panel.SetActive(false);
        if(player!=null)player.ReleaseMovementLock(this);
        if(wasOpen && !SceneFade.IsRunning)GameAudio.Play(GameAudio.Cue.Close);
    }
    private void Refresh()
    {
        if (controls != null) controls.text = "W A S D  mover    •    Clique para interagir    •    I  inventário    •    " +
            (mission != null && mission.HasMap ? "M  mapa    •    " : "") + "Esc  pausa";
        if (mission != null && !mission.HasMap && IsOpen) Close();
        if(!IsOpen || mission==null)return;
        string current=MapScene(SceneManager.GetActiveScene().name);
        string[] objectiveLines = story.ObjectiveText.Split('\n');
        objective.text="<size=14><color=#835124>PRÓXIMO OBJETIVO</color></size>\n" + objectiveLines[objectiveLines.Length-1];
        for(int i=0;i<IslandMapGraphic.Locations.Length;i++)
        {
            string scene=IslandMapGraphic.Locations[i];bool visited=mission.HasVisitedLocation(scene);
            pins[i].color=scene==selected ? new Color(.96f,.77f,.4f) : visited ? new Color(.20f,.39f,.39f) : new Color(.25f,.29f,.30f);
            pinLabels[i].color=scene==selected ? new Color(.07f,.12f,.14f) : new Color(.97f,.94f,.82f);
            pinLabels[i].text=LocationName(scene)+"\n<size=12>"+(scene==current ? "VOCÊ ESTÁ AQUI" : scene==mission.ObjectiveScene ? "OBJETIVO" : visited ? "VISITADO" : "NÃO VISITADO")+"</size>";
        }
        int index=System.Array.IndexOf(IslandMapGraphic.Locations,selected);
        if(index<0)return;
        bool known=mission.HasVisitedLocation(selected);
        preview.texture=previews[index];preview.color=known?Color.white:new Color(.35f,.40f,.43f);
        detailTitle.text=LocationName(selected).ToUpperInvariant();
        detailText.text=selected==current ? "Você está aqui. Escolha outro lugar visitado para viajar." : known ? "Você já conhece o caminho. Pode viajar para este lugar." : "Explore este lugar pelo cenário para liberar a viagem rápida.";
        if(selected==mission.ObjectiveScene)detailText.text+="\n\n<color=#835124>Seu próximo objetivo está aqui.</color>";
        travelLabel.text=selected==current ? "VOCÊ ESTÁ AQUI" : !known ? "VISITE PRIMEIRO" : "VIAJAR";
        travelButton.interactable=CanTravel;
    }
    private void OnDisable() => Close();
    private void OnDestroy(){if(mission!=null)mission.OnMissionStateChanged-=Refresh;}
}

