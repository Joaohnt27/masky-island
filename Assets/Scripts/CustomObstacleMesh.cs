using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class CustomObstacleMesh : MonoBehaviour
{
    [Header("Formato do obstáculo")]
    [SerializeField]
    private Vector2[] points =
    {
        new Vector2(-0.8f, -0.4f),
        new Vector2(0.8f, -0.4f),
        new Vector2(0.8f, 0.4f),
        new Vector2(-0.8f, 0.4f)
    };

    [Header("Espessura")]
    [SerializeField] private float depth = 0.2f;

    public Vector2[] Points => points;
    public float Depth => depth;

    private void OnEnable()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        if (points == null || points.Length < 3)
            return;

        Mesh mesh = new Mesh();
        mesh.name = "ObstacleCollisionMesh";

        int count = points.Length;

        Vector3[] vertices = new Vector3[count * 2];

        // Frente
        for (int i = 0; i < count; i++)
        {
            vertices[i] = new Vector3(
                points[i].x,
                0,
                points[i].y - depth / 2
            );
        }

        // Trás
        for (int i = 0; i < count; i++)
        {
            vertices[count + i] = new Vector3(
                points[i].x,
                0,
                points[i].y + depth / 2
            );
        }

        int triangleCount = (count - 2) * 2 + count * 2;

        int[] triangles = new int[triangleCount * 3];

        int index = 0;

        // Frente
        for (int i = 1; i < count - 1; i++)
        {
            triangles[index++] = 0;
            triangles[index++] = i + 1;
            triangles[index++] = i;
        }

        // Trás
        for (int i = 1; i < count - 1; i++)
        {
            triangles[index++] = count;
            triangles[index++] = count + i;
            triangles[index++] = count + i + 1;
        }

        // Laterais
        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            triangles[index++] = i;
            triangles[index++] = count + i;
            triangles[index++] = next;

            triangles[index++] = next;
            triangles[index++] = count + i;
            triangles[index++] = count + next;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshFilter.sharedMesh = mesh;

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }
}