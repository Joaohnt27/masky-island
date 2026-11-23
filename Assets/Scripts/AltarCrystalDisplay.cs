using UnityEngine;

// Visual do altar independente do painel. O progresso pertence ao GameManager.
public class AltarCrystalDisplay : MonoBehaviour
{
    private CrystalSymbolGraphic[] crystals;
    private Vector3[] positions;
    private Canvas canvas;

    public void Initialize(CrystalSymbolGraphic[] graphics, Vector3[] worldPositions)
    {
        crystals = graphics;
        positions = worldPositions;
        canvas = GetComponentInParent<Canvas>();
        Refresh();
    }

    private void OnEnable() => Canvas.willRenderCanvases += Refresh;
    private void OnDisable() => Canvas.willRenderCanvases -= Refresh;
    private void LateUpdate() => Refresh();

    private void Refresh()
    {
        if (crystals == null) return;
        var mission = GameManager.Instance;
        var camera = Camera.main;
        var uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        for (int i = 0; i < crystals.Length; i++)
        {
            bool filled = mission != null && mission.IsCrystalSocketFilled(i);
            crystals[i].gameObject.SetActive(filled);
            if (filled && camera != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform, camera.WorldToScreenPoint(positions[i]), uiCamera, out var point))
                crystals[i].rectTransform.anchoredPosition = point;
        }
    }
}
