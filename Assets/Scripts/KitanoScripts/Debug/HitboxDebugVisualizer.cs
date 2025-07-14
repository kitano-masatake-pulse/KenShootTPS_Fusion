using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class HitboxDebugVisualizer : MonoBehaviour
{
    public static HitboxDebugVisualizer Instance { get; private set; }

    [Header("表示切り替え")]
    public bool isVisualized = false;

    [Header("色とスケール")]
    public Color wireColor = Color.cyan;
    public float scaleMultiplier = 1f;

    private List<HitboxRoot> registeredRoots = new();

    private static Material lineMat;
    private static Mesh wireCubeMesh;
    private static Mesh wireSphereMesh;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (lineMat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader);
            lineMat.SetInt("_ZWrite", 0);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        wireCubeMesh ??= BuildWireCubeMesh();
        wireSphereMesh ??= BuildWireSphereMesh();
    }

    public void Register(HitboxRoot root)
    {
        if (!registeredRoots.Contains(root))
        {
            registeredRoots.Add(root);
        }
    }

    public void Unregister(HitboxRoot root)
    {
        if (registeredRoots.Contains(root))
        {
            registeredRoots.Remove(root);
        }
    }

    private void OnRenderObject()
    {

        isVisualized= SROptions.Current.isShowingHitbox; // SROptionsからのフラグ取得

        if (!isVisualized) return;

        lineMat.SetColor("_Color", wireColor);
        lineMat.SetPass(0);

        foreach (var root in registeredRoots)
        {
            foreach (var hb in root.Hitboxes)
            {
                var t = hb.transform;
                Matrix4x4 world = Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
                Matrix4x4 matrix;

                if (hb.Type == HitboxTypes.Sphere)
                {
                    float r = hb.SphereRadius * scaleMultiplier;
                    matrix = world * Matrix4x4.TRS(hb.Offset, Quaternion.identity, Vector3.one * r * 2f);
                    Graphics.DrawMeshNow(wireSphereMesh, matrix);
                }
                else if (hb.Type == HitboxTypes.Box)
                {
                    Vector3 size = hb.BoxExtents * 2f * scaleMultiplier;
                    matrix = world * Matrix4x4.TRS(hb.Offset, Quaternion.identity, size);
                    Graphics.DrawMeshNow(wireCubeMesh, matrix);
                }
            }
        }
    }

    // Wire mesh generators (略) 以前のコードのまま使えます
    private Mesh BuildWireCubeMesh()
    {
        var mesh = new Mesh();
        var lines = new[]
        {
            // bottom square
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f),

            // top square
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f),

            // vertical lines
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
        };
        mesh.vertices = lines;
        int[] indices = new int[lines.Length];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        return mesh;
    }

    private Mesh BuildWireSphereMesh()
    {
        int latSegments = 10;   // 緯線（水平）分割数
        int lonSegments = 10;   // 経線（垂直）分割数
        float radius = 0.5f;

        var vertices = new List<Vector3>();
        var indices = new List<int>();

        // 緯線（横切る輪）
        for (int i = 1; i < latSegments; i++)
        {
            float lat = Mathf.PI * i / latSegments;
            float y = Mathf.Cos(lat) * radius;
            float r = Mathf.Sin(lat) * radius;

            for (int j = 0; j < lonSegments; j++)
            {
                float lon1 = 2 * Mathf.PI * j / lonSegments;
                float lon2 = 2 * Mathf.PI * (j + 1) / lonSegments;

                Vector3 p1 = new Vector3(Mathf.Cos(lon1) * r, y, Mathf.Sin(lon1) * r);
                Vector3 p2 = new Vector3(Mathf.Cos(lon2) * r, y, Mathf.Sin(lon2) * r);

                vertices.Add(p1);
                vertices.Add(p2);
                indices.Add(vertices.Count - 2);
                indices.Add(vertices.Count - 1);
            }
        }

        // 経線（縦方向）
        for (int i = 0; i < lonSegments; i++)
        {
            float lon = 2 * Mathf.PI * i / lonSegments;

            for (int j = 0; j < latSegments; j++)
            {
                float lat1 = Mathf.PI * j / latSegments;
                float lat2 = Mathf.PI * (j + 1) / latSegments;

                Vector3 p1 = new Vector3(
                    Mathf.Sin(lat1) * Mathf.Cos(lon),
                    Mathf.Cos(lat1),
                    Mathf.Sin(lat1) * Mathf.Sin(lon)
                ) * radius;

                Vector3 p2 = new Vector3(
                    Mathf.Sin(lat2) * Mathf.Cos(lon),
                    Mathf.Cos(lat2),
                    Mathf.Sin(lat2) * Mathf.Sin(lon)
                ) * radius;

                vertices.Add(p1);
                vertices.Add(p2);
                indices.Add(vertices.Count - 2);
                indices.Add(vertices.Count - 1);
            }
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        return mesh;
    }
}
