using UnityEngine;
using UnityEngine.UI;

// Partículas de UI: visíveis sobre o background do Canvas Overlay, sem materiais extras.
public class SplashParticles : MaskableGraphic
{
    private struct Mote
    {
        public float x, y, speed, size, phase;
    }

    private Mote[] motes;
    private float elapsed;
    private float clickAge = -1f;
    private float clickDuration;
    private Vector2 clickPoint;

    public void Initialize(int count, Color tint)
    {
        raycastTarget = false;
        color = tint;
        motes = new Mote[Mathf.Clamp(count, 10, 100)];
        // Não altera o gerador global usado pelas recompensas da mineração.
        var random = new System.Random(417);
        for (int i = 0; i < motes.Length; i++)
            motes[i] = new Mote
            {
                x = (float)random.NextDouble(),
                y = (float)random.NextDouble(),
                speed = Mathf.Lerp(0.015f, 0.045f, (float)random.NextDouble()),
                size = Mathf.Lerp(3f, 8f, (float)random.NextDouble()),
                phase = (float)random.NextDouble() * Mathf.PI * 2f
            };
        SetVerticesDirty();
    }

    public void PlayClick(Vector2 screenPoint, Camera camera, float duration)
    {
        if (clickAge >= 0f) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out clickPoint);
        clickAge = 0f;
        clickDuration = Mathf.Max(0.3f, duration);
        SetVerticesDirty();
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        if (clickAge >= 0f) clickAge += Time.unscaledDeltaTime;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect bounds = rectTransform.rect;
        if (motes == null || bounds.width <= 0f || bounds.height <= 0f) return;
        // Tamanhos proporcionais à tela, independentemente do CanvasScaler atual.
        float scale = Mathf.Min(bounds.width / 1920f, bounds.height / 1080f);
        foreach (var mote in motes)
        {
            float x = Mathf.Repeat(mote.x + Mathf.Sin(elapsed * 0.3f + mote.phase) * 0.018f, 1f);
            float y = Mathf.Repeat(mote.y + elapsed * mote.speed, 1f);
            float edgeFade = Mathf.Clamp01(Mathf.Min(y, 1f - y) * 12f);
            var tint = color;
            tint.a *= edgeFade * (0.4f + 0.6f * Mathf.Pow(Mathf.Sin(elapsed + mote.phase), 2f));
            Glow(mesh, new Vector2(bounds.xMin + x * bounds.width, bounds.yMin + y * bounds.height),
                mote.size * scale, tint);
        }

        if (clickAge < 0f) return;
        float progress = Mathf.Clamp01(clickAge / clickDuration);
        float expansion = 1f - Mathf.Pow(1f - progress, 3f);
        var burstColor = color;
        burstColor.a = (1f - progress) * 0.95f;
        Ring(mesh, clickPoint, Mathf.Lerp(8f, 165f, expansion) * scale, 2f * scale, burstColor);
        for (int i = 0; i < 24; i++)
        {
            float angle = i * Mathf.PI * 2f / 24f;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float distance = expansion * (i % 2 == 0 ? 145f : 95f) * scale;
            Glow(mesh, clickPoint + direction * distance, (7f - 4f * progress) * scale, burstColor);
        }
        // O efeito permanece visível antes de a tela escurecer e carregar MainMenu.
        float fade = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, progress));
        int start = mesh.currentVertCount;
        Color shade = new Color(0.015f, 0.025f, 0.045f, fade);
        mesh.AddVert(new Vector2(bounds.xMin, bounds.yMin), shade, Vector2.zero);
        mesh.AddVert(new Vector2(bounds.xMin, bounds.yMax), shade, Vector2.zero);
        mesh.AddVert(new Vector2(bounds.xMax, bounds.yMax), shade, Vector2.zero);
        mesh.AddVert(new Vector2(bounds.xMax, bounds.yMin), shade, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2);
        mesh.AddTriangle(start, start + 2, start + 3);
    }

    private static void Glow(VertexHelper mesh, Vector2 center, float radius, Color tint)
    {
        const int segments = 16;
        int start = mesh.currentVertCount;
        mesh.AddVert(center, tint, Vector2.zero);
        tint.a = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, tint, Vector2.zero);
            if (i > 0) mesh.AddTriangle(start, start + i, start + i + 1);
        }
    }

    private static void Ring(VertexHelper mesh, Vector2 center, float radius, float width, Color tint)
    {
        const int segments = 64;
        int start = mesh.currentVertCount;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            mesh.AddVert(center + direction * radius, tint, Vector2.zero);
            mesh.AddVert(center + direction * Mathf.Max(0f, radius - width), tint, Vector2.zero);
            if (i == 0) continue;
            int end = start + i * 2;
            mesh.AddTriangle(end - 2, end - 1, end);
            mesh.AddTriangle(end, end - 1, end + 1);
        }
    }
}
