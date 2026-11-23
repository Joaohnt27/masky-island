using UnityEngine;
using UnityEngine.UI;

// Ilustração opcional com forma vetorial como fallback para cenas antigas.
[RequireComponent(typeof(CanvasRenderer))]
public class CrystalSymbolGraphic : MaskableGraphic
{
    [SerializeField] private GameManager.CrystalId shape;
    [SerializeField] private Texture2D illustration;
    public override Texture mainTexture => illustration != null ? illustration : base.mainTexture;
    public GameManager.CrystalId Shape { get => shape; set { shape = value; SetVerticesDirty(); } }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        if (illustration != null)
        {
            float side = Mathf.Min(rect.width, rect.height);
            var imageCenter = rect.center;
            var tint = new Color(1, 1, 1, color.a);
            mesh.AddVert(imageCenter + new Vector2(-side/2,-side/2),tint,new Vector2(0,0));
            mesh.AddVert(imageCenter + new Vector2(-side/2,side/2),tint,new Vector2(0,1));
            mesh.AddVert(imageCenter + new Vector2(side/2,side/2),tint,new Vector2(1,1));
            mesh.AddVert(imageCenter + new Vector2(side/2,-side/2),tint,new Vector2(1,0));
            mesh.AddTriangle(0,1,2);mesh.AddTriangle(0,2,3);return;
        }
        float radius = Mathf.Min(rect.width, rect.height) * 0.48f;
        int sides = shape == GameManager.CrystalId.Circle ? 40 : shape == GameManager.CrystalId.Triangle ? 3 : 4;
        float rotation = shape == GameManager.CrystalId.Square ? 45f : 90f;
        Vector2 center = rect.center;
        for (int i = 0; i < sides; i++)
        {
            float a = (rotation + i * 360f / sides) * Mathf.Deg2Rad;
            float b = (rotation + (i + 1) * 360f / sides) * Mathf.Deg2Rad;
            Vector2 edgeA = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            Vector2 edgeB = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
            AddTriangle(mesh, center, center + edgeA * radius, center + edgeB * radius,
                Color.Lerp(color, Color.white, 0.55f));
            Color facet = Color.Lerp(color, i % 2 == 0 ? Color.white : Color.black, i % 2 == 0 ? 0.22f : 0.12f);
            facet.a = color.a;
            AddTriangle(mesh, center + new Vector2(-radius * 0.12f, radius * 0.15f),
                center + edgeA * radius * 0.88f, center + edgeB * radius * 0.88f, facet);
        }
    }

    private static void AddTriangle(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Color tint)
    {
        int index = mesh.currentVertCount;
        mesh.AddVert(a, tint, Vector2.zero);
        mesh.AddVert(b, tint, Vector2.zero);
        mesh.AddVert(c, tint, Vector2.zero);
        mesh.AddTriangle(index, index + 1, index + 2);
    }
}
