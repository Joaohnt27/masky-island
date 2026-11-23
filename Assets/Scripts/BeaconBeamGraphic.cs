using UnityEngine;
using UnityEngine.UI;

// Feixes translúcidos usados pelo encerramento ilustrado.
[RequireComponent(typeof(CanvasRenderer))]
public class BeaconBeamGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        Color edge = color; edge.a = 0;
        vh.AddVert(new Vector3(0, 0), color, Vector2.zero);
        vh.AddVert(new Vector3(r.xMax, r.yMax), edge, Vector2.zero);
        vh.AddVert(new Vector3(r.xMax, r.yMin), edge, Vector2.zero);
        vh.AddVert(new Vector3(r.xMin, r.yMax), edge, Vector2.zero);
        vh.AddVert(new Vector3(r.xMin, r.yMin), edge, Vector2.zero);
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 4, 3);
        Color core = color; core.a = Mathf.Clamp01(color.a * 2.5f);
        int center = vh.currentVertCount;
        vh.AddVert(Vector3.zero, core, Vector2.zero);
        const int segments = 32;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            vh.AddVert(new Vector3(Mathf.Cos(angle) * 42, Mathf.Sin(angle) * 42), core, Vector2.zero);
            if (i > 0) vh.AddTriangle(center, center + i + 1, center + i);
        }
    }
}
