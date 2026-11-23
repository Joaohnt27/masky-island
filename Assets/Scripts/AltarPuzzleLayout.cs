using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Mantém os botões do puzzle alinhados aos encaixes pintados no cenário.
public class AltarPuzzleLayout : MonoBehaviour
{
    [SerializeField] private RectTransform[] slots;
    [SerializeField] private Vector3[] worldPositions;
    [SerializeField] private TMP_Text feedback;
    [SerializeField] private GameObject socketGroup;
    private bool placedCrystalsInitialized;

    public void InitializePlacedCrystals()
    {
        if (placedCrystalsInitialized) return;
        placedCrystalsInitialized = true;
        var host = new GameObject("PlacedAltarCrystals", typeof(RectTransform));
        host.transform.SetParent(transform.parent, false);
        host.transform.SetSiblingIndex(transform.GetSiblingIndex());
        var rect = (RectTransform)host.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        var graphics = new CrystalSymbolGraphic[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            var original = slots[i].GetComponentInChildren<CrystalSymbolGraphic>(true);
            graphics[i] = Instantiate(original, host.transform, false);
            graphics[i].name = "PlacedCrystal" + i;
            graphics[i].raycastTarget = false;
            original.enabled = false;
        }
        host.AddComponent<AltarCrystalDisplay>().Initialize(graphics, worldPositions);
    }
    private void OnEnable() => Canvas.willRenderCanvases += Align;
    private void OnDisable() => Canvas.willRenderCanvases -= Align;
    private void LateUpdate() => Align();
    private void Align()
    {
        if(Camera.main==null)return;
        var canvas=GetComponentInParent<Canvas>();
        Camera uiCamera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
        for(int i=0;i<slots.Length&&i<worldPositions.Length;i++)
            if(slots[i]!=null&&RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)slots[i].parent,Camera.main.WorldToScreenPoint(worldPositions[i]),uiCamera,out var point))
            {
                slots[i].anchoredPosition=point;
                var surface=slots[i].GetComponent<Image>();
                if(surface!=null)surface.color=Color.clear;
                var outline=slots[i].GetComponent<Outline>();
                if(outline!=null)outline.enabled=false;
            }
        if(feedback!=null){feedback.rectTransform.anchoredPosition=new Vector2(0,-305);feedback.rectTransform.sizeDelta=new Vector2(660,50);feedback.fontSize=18;}
        if(GameManager.Instance!=null&&GameManager.Instance.HasMemory(GameManager.MemoryId.StormNight))socketGroup.SetActive(true);
    }
}
