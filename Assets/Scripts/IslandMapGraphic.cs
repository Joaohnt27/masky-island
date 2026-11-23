using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class IslandMapGraphic : MaskableGraphic
{
    public static readonly string[] Locations = { "Beach", "House", "Island", "Cave", "LighthouseExterior" };
    public static readonly Vector2[] Points = { new Vector2(-.30f,-.32f), new Vector2(-.34f,.02f), Vector2.zero, new Vector2(.32f,.06f), new Vector2(.13f,.34f) };
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Vector2[] coast = {new Vector2(-.47f,-.25f),new Vector2(-.32f,-.47f),new Vector2(.05f,-.43f),new Vector2(.40f,-.27f),new Vector2(.47f,.12f),new Vector2(.24f,.47f),new Vector2(-.12f,.45f),new Vector2(-.44f,.25f)};
        Polygon(vh,coast,1.06f,new Color(.22f,.44f,.45f));
        Polygon(vh,coast,1,new Color(.73f,.68f,.47f));
        Polygon(vh,coast,.91f,new Color(.40f,.51f,.39f));
        Path(vh,0,1);Path(vh,1,2);Path(vh,0,2);Path(vh,2,3);Path(vh,2,4);
    }
    private Vector2 Position(Vector2 v) => new Vector2(v.x*rectTransform.rect.width,v.y*rectTransform.rect.height);
    private void Polygon(VertexHelper vh,Vector2[] points,float scale,Color tint)
    {
        int start=vh.currentVertCount;vh.AddVert(Vector3.zero,tint,Vector2.zero);
        foreach(var point in points) vh.AddVert(Position(point*scale),tint,Vector2.zero);
        for(int i=0;i<points.Length;i++)vh.AddTriangle(start,start+1+(i+1)%points.Length,start+1+i);
    }
    private void Path(VertexHelper vh,int from,int to)
    {
        Vector2 a=Position(Points[from]),b=Position(Points[to]);
        int dots=Mathf.Max(2,Mathf.RoundToInt(Vector2.Distance(a,b)/12));
        for(int i=0;i<=dots;i++)
        {
            Vector2 p=Vector2.Lerp(a,b,(float)i/dots);int n=vh.currentVertCount;
            Color tint=new Color(.95f,.84f,.60f);
            vh.AddVert(p+new Vector2(-2,-2),tint,Vector2.zero);vh.AddVert(p+new Vector2(-2,2),tint,Vector2.zero);
            vh.AddVert(p+new Vector2(2,2),tint,Vector2.zero);vh.AddVert(p+new Vector2(2,-2),tint,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}

