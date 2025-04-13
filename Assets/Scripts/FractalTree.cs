using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FractalTree : MonoBehaviour
{
    public int depth = 5;
    public float length = 1f;
    public float angle = 30f;
    public float lengthScale = 0.7f;

    private Mesh mesh;
    private List<Vector3> vertices;
    private List<int> triangles;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void Update()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.up * length;
        DrawBranch(start, end, depth, 0f);

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }

    
    void DrawBranch(Vector3 start, Vector3 end, int currentDepth, float timeOffset)
    {
        if (currentDepth == 0)
            return;

        AddQuad(start, end, 0.05f);

        Vector3 direction = end - start;

        // 흔들림 각도 계산
        float t = Time.time;
        float frequency = 1f;
        float amplitude = 10f * ((float)currentDepth / depth); // 깊이에 따라 흔들림 줄이기
        float windAngle = Mathf.Sin(t * frequency + timeOffset) * amplitude;

        // 왼/오 방향 회전 적용
        Vector3 rightDir = Quaternion.Euler(0, 0, angle + windAngle) * direction.normalized;
        Vector3 leftDir = Quaternion.Euler(0, 0, -angle + windAngle) * direction.normalized;

        float nextLength = direction.magnitude * lengthScale;
        Vector3 rightEnd = end + rightDir * nextLength;
        Vector3 leftEnd = end + leftDir * nextLength;

        float nextOffset = timeOffset + 0.5f; // depth마다 offset 다르게

        DrawBranch(end, rightEnd, currentDepth - 1, nextOffset);
        DrawBranch(end, leftEnd, currentDepth - 1, -nextOffset);
    }

    void AddQuad(Vector3 start, Vector3 end, float width)
    {
        Vector3 dir = (end - start).normalized;
        Vector3 normal = new Vector3(-dir.y, dir.x, 0) * width;

        Vector3 v0 = start - normal;
        Vector3 v1 = start + normal;
        Vector3 v2 = end - normal;
        Vector3 v3 = end + normal;

        int index = vertices.Count;

        vertices.Add(v0);
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index + 0);
        triangles.Add(index + 1);
        triangles.Add(index + 2);

        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index + 3);
    }
}
