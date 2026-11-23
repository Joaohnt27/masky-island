using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Apresentação local; o inventário continua armazenado no GameManager persistente.
public class CrystalInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private CrystalSymbolGraphic[] icons;
    [SerializeField] private TMP_Text[] labels;
    [SerializeField] private GameObject keyHint;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;
    [SerializeField] private StoryLocation story;
    [System.Serializable] public struct ItemVisual { public GameManager.ItemId item; public Texture texture; public Rect uv; }
    [SerializeField] private ItemVisual[] itemVisuals;
    [SerializeField] private RawImage[] itemIcons;
    [SerializeField] private TMP_Text[] itemLabels;
    [SerializeField] private Button[] itemButtons;
    [SerializeField] private TMP_Text itemDescription;
    [SerializeField] private bool illustratedBag;
    [SerializeField] private RawImage detailIcon;
    private readonly System.Collections.Generic.List<GameManager.ItemId> displayed = new System.Collections.Generic.List<GameManager.ItemId>();
    private GameManager.ItemId selected;
    private void Awake(){if(itemButtons!=null)for(int i=0;i<itemButtons.Length;i++){int index=i;itemButtons[i].onClick.AddListener(()=>{if(index<displayed.Count){selected=displayed[index];Refresh();}});}}
    private GameManager mission;
    private PlayerClickMovement player;
    public bool IsOpen { get; private set; }

    private void Start() => Bind();
    private void OnEnable()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        Bind();
    }
    private void Update()
    {
        if (IsOpen && (SceneFade.IsRunning || mission == null || mission.BlocksGameplay)) Close();
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame) Toggle();
        if (openButton != null)
        {
            bool visible = mission != null && mission.HasBackpack && !mission.BlocksGameplay &&
                !IsOpen && (story == null || !story.HasFocusPanel);
            openButton.gameObject.SetActive(visible);
            openButton.interactable = visible && !SceneFade.IsRunning && player != null && player.IsMovementEnabled;
        }
    }
    public void Toggle()
    {
        if (IsOpen) { Close(); return; }
        if (mission == null || !mission.HasBackpack || mission.BlocksGameplay || SceneFade.IsRunning) return;
        if (player == null) player = FindAnyObjectByType<PlayerClickMovement>();
        if (player == null || !player.IsMovementEnabled) return;
        IsOpen = true;
        GameAudio.Play(GameAudio.Cue.Open);
        player.AcquireMovementLock(this);
        Refresh();
    }
    public void Close()
    {
        if (IsOpen && !SceneFade.IsRunning) GameAudio.Play(GameAudio.Cue.Close);
        IsOpen = false;
        if (player != null) player.ReleaseMovementLock(this);
        if (panel != null) panel.SetActive(false);
    }
    private void Bind()
    {
        if (player == null) player = FindAnyObjectByType<PlayerClickMovement>();
        if (mission != null) mission.OnMissionStateChanged -= Refresh;
        mission = GameManager.Instance;
        if (mission != null) mission.OnMissionStateChanged += Refresh;
        Refresh();
    }
    private void Refresh()
    {
        if (panel == null) return;
        bool availableMission = mission != null && mission.HasBackpack;
        if (!availableMission || mission.BlocksGameplay) Close();
        panel.SetActive(availableMission && IsOpen);
        if (keyHint != null) keyHint.SetActive(availableMission && !mission.BlocksGameplay);
        if (mission == null) return;
        displayed.Clear();
        foreach(GameManager.ItemId id in System.Enum.GetValues(typeof(GameManager.ItemId)))if(mission.GetItemCount(id)>0)displayed.Add(id);
        if(displayed.Count>0&&!displayed.Contains(selected))selected=displayed[0];
        if(title!=null)title.text="MOCHILA  •  "+displayed.Count+" TIPOS DE ITENS";
        if(itemButtons==null)return;
        for(int i=0;i<itemButtons.Length;i++){
            bool occupied=i<displayed.Count;itemButtons[i].interactable=occupied;
            itemIcons[i].gameObject.SetActive(occupied);
            itemLabels[i].text=occupied ? InventoryItemInfo.Name(displayed[i])+"  ×"+mission.GetItemCount(displayed[i]) : "Espaço livre";
            itemButtons[i].GetComponent<Image>().color=illustratedBag ? (occupied&&displayed[i]==selected?new Color(1f,.80f,.34f,.24f):new Color(1,1,1,.01f)) : (occupied&&displayed[i]==selected?new Color(.84f,.69f,.42f):new Color(.94f,.87f,.70f));
            if(occupied)foreach(var visual in itemVisuals)if(visual.item==displayed[i]){itemIcons[i].texture=visual.texture;itemIcons[i].uvRect=visual.uv;float ratio=visual.texture.width*visual.uv.width/(visual.texture.height*visual.uv.height);float size=illustratedBag?72:92;itemIcons[i].rectTransform.sizeDelta=ratio>1?new Vector2(size,size/ratio):new Vector2(size*ratio,size);break;}
        }
        if(detailIcon!=null){detailIcon.gameObject.SetActive(displayed.Count>0);if(displayed.Count>0)foreach(var visual in itemVisuals)if(visual.item==selected){detailIcon.texture=visual.texture;detailIcon.uvRect=visual.uv;float ratio=visual.texture.width*visual.uv.width/(visual.texture.height*visual.uv.height);detailIcon.rectTransform.sizeDelta=ratio>1?new Vector2(100,100/ratio):new Vector2(100*ratio,100);break;}}
        if(itemDescription!=null)itemDescription.text=displayed.Count==0?"Sua mochila está vazia. Colete itens pelo cenário.":InventoryItemInfo.Name(selected)+"\n"+InventoryItemInfo.Description(selected);
    }

    private void OnDisable()
    {
        Close();
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (keyHint != null) keyHint.SetActive(false);
        if (mission != null) mission.OnMissionStateChanged -= Refresh;
        if (panel != null) panel.SetActive(false);
    }
}
