using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(HitboxRoot))]
public class HitboxWireframeDrawer : MonoBehaviour
{
    [Header("表示フラグ")]
    public bool isVisualized = true;

    [Header("色設定")]
    public Color wireColor = Color.yellow;

    [Header("スケール倍率（調整用）")]
    public float scaleMultiplier = 1f;

    private HitboxRoot _hitboxRoot;
    private List<Hitbox> _hitboxes = new();

    private static Material _lineMaterial;
    private static Mesh _wireCubeMesh;
    private static Mesh _wireSphereMesh;

    private void Awake()
    {
        _hitboxRoot = GetComponent<HitboxRoot>();
        _hitboxes = _hitboxRoot.Hitboxes.ToList();

        if (_lineMaterial == null)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            _lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            _lineMaterial.SetInt("_ZWrite", 0);
            _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }

        if (_wireCubeMesh == null)
            _wireCubeMesh = BuildWireCubeMesh();

        if (_wireSphereMesh == null)
            _wireSphereMesh = BuildWireSphereMesh();
    }

    private void OnRenderObject()
    {
        isVisualized= SROptions.Current.isShowingHitbox; // SROptionsからのフラグ取得
        if (!isVisualized || _hitboxes == null) return;

        _lineMaterial.color = wireColor;      
        _lineMaterial.SetPass(0);
        //GL.Color(wireColor);

        foreach (var hb in _hitboxes)
        {
            var t = hb.transform;

            Matrix4x4 world = Matrix4x4.TRS(t.position, t.rotation, Vector3.one);

            switch (hb.Type)
            {
                case HitboxTypes.Sphere:
                    {
                        Vector3 offset = hb.Offset;
                        float r = hb.SphereRadius * scaleMultiplier;
                        var matrix = world * Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * r * 2f);
                        Graphics.DrawMeshNow(_wireSphereMesh, matrix);
                        break;
                    }

                case HitboxTypes.Box:
                    {
                        Vector3 offset = hb.Offset;
                        Vector3 size = hb.BoxExtents * 2f * scaleMultiplier;
                        var matrix = world * Matrix4x4.TRS(offset, Quaternion.identity, size);
                        Graphics.DrawMeshNow(_wireCubeMesh, matrix);
                        break;
                    }

                default:
                    Debug.LogWarning($"{hb.name}：未対応のHitboxTypeです。");
                    break;
            }
        }
    }

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
