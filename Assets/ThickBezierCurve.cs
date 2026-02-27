using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ThickBezierCurve : MonoBehaviour {
    [System.Serializable]
    public struct ControlPoint {
        public Vector3 position;
        public float thickness;
    }

    public ControlPoint[] controlPoints;
    public int segmentsPerCurve = 20;
    public int tubeSegments = 8; // radial segments around the tube

    private Mesh mesh;

    void Start() => GenerateMesh();
    void OnValidate() => GenerateMesh();

    void GenerateMesh() {
        if (controlPoints == null || controlPoints.Length < 4) return;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mesh == null) mesh = new Mesh();
        mesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Process each cubic bezier segment (every 3 points: P0, P1, P2, P3)
        int numCurves = (controlPoints.Length - 1) / 3;

        for (int c = 0; c < numCurves; c++) {
            int baseIndex = c * 3;
            ControlPoint p0 = controlPoints[baseIndex];
            ControlPoint p1 = controlPoints[baseIndex + 1];
            ControlPoint p2 = controlPoints[baseIndex + 2];
            ControlPoint p3 = controlPoints[baseIndex + 3];

            for (int i = 0; i <= segmentsPerCurve; i++) {
                float t = i / (float)segmentsPerCurve;

                // Cubic Bezier position
                Vector3 pos = CubicBezier(p0.position, p1.position, p2.position, p3.position, t);

                // Interpolated thickness
                float radius = Mathf.Lerp(p0.thickness, p3.thickness, t) * 0.5f;

                // Tangent for orientation
                Vector3 tangent = CubicBezierTangent(p0.position, p1.position, p2.position, p3.position, t).normalized;
                Vector3 up = Vector3.up;
                if (Vector3.Dot(tangent, up) > 0.99f) up = Vector3.right;
                Vector3 right = Vector3.Cross(tangent, up).normalized;
                up = Vector3.Cross(right, tangent).normalized;

                // Ring of vertices
                for (int j = 0; j < tubeSegments; j++) {
                    float angle = j / (float)tubeSegments * Mathf.PI * 2f;
                    Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * radius;
                    vertices.Add(pos + offset);
                    uvs.Add(new Vector2(j / (float)tubeSegments, t));
                }
            }
        }

        // Build triangles
        int rings = (segmentsPerCurve * numCurves) + numCurves;
        for (int ring = 0; ring < rings; ring++) {
            for (int j = 0; j < tubeSegments; j++) {
                int current = ring * tubeSegments + j;
                int next = ring * tubeSegments + (j + 1) % tubeSegments;
                int currentNext = (ring + 1) * tubeSegments + j;
                int nextNext = (ring + 1) * tubeSegments + (j + 1) % tubeSegments;

                triangles.Add(current);
                triangles.Add(currentNext);
                triangles.Add(next);

                triangles.Add(next);
                triangles.Add(currentNext);
                triangles.Add(nextNext);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        float u = 1 - t;
        return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
    }

    Vector3 CubicBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        float u = 1 - t;
        return 3 * u * u * (p1 - p0) + 6 * u * t * (p2 - p1) + 3 * t * t * (p3 - p2);
    }
}