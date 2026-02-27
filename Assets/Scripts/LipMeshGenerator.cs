using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LipMeshGenerator : MonoBehaviour {
    [Header("Curve Control Points")]
    [Tooltip("Left edge point (world-relative)")]
    public Vector3 leftPoint = new Vector3(-1f, 0f, 0f);

    [Tooltip("Right edge point (world-relative)")]
    public Vector3 rightPoint = new Vector3(1f, 0f, 0f);

    [Tooltip("Top peak control point")]
    public Vector3 topPoint = new Vector3(0f, 0.5f, 0f);

    [Header("Lip Thickness")]
    [Tooltip("Thickness at the center of the lip")]
    public float centerThickness = 0.15f;

    [Tooltip("Thickness at the edges of the lip")]
    public float edgeThickness = 0.01f;

    [Tooltip("How quickly thickness falls off toward edges (higher = sharper taper)")]
    [Range(0.5f, 4f)]
    public float thicknessFalloff = 2f;

    [Header("Color")]
    [Tooltip("Drag your material here. Its color property will be updated with Lip Color.")]
    public Material lipMaterial;

    [Tooltip("Color to apply to the assigned material")]
    public Color lipColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    [Header("Resolution")]
    [Tooltip("Number of segments along the curve")]
    [Range(4, 128)]
    public int segments = 32;

    [Header("Vertex Weights (per segment, 0=edge, 1=center)")]
    [Tooltip("Manually override the thickness weight for each segment column. Size must match segments+1.")]
    public float[] vertexWeights;

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Awake() {
        GenerateMesh();
    }

    public void OnValidate() {
        int count = segments + 1;
        if (vertexWeights == null || vertexWeights.Length != count) {
            float[] newWeights = new float[count];
            for (int i = 0; i < count; i++) {
                float t = (float)i / segments;
                newWeights[i] = Mathf.Pow(Mathf.Sin(t * Mathf.PI), thicknessFalloff);
            }
            vertexWeights = newWeights;
        }

        if (Application.isPlaying)
            GenerateMesh();
    }

    Vector3 Bezier(float t) {
        float u = 1f - t;
        return u * u * leftPoint + 2f * u * t * topPoint + t * t * rightPoint;
    }

    Vector3 BezierTangent(float t) {
        return 2f * (1f - t) * (topPoint - leftPoint) + 2f * t * (rightPoint - topPoint);
    }

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh() {
        meshFilter = GetComponent<MeshFilter>();
        if (mesh == null) {
            mesh = new Mesh();
            mesh.name = "LipMesh";
        }
        mesh.Clear();

        int vertCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[segments * 6];

        if (vertexWeights == null || vertexWeights.Length != segments + 1)
            OnValidate();

        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;

            Vector3 center = Bezier(t);
            Vector3 tangent = BezierTangent(t).normalized;
            Vector3 normal = new Vector3(-tangent.y, tangent.x, 0f).normalized;

            float weight = (vertexWeights != null && i < vertexWeights.Length)
                ? vertexWeights[i]
                : Mathf.Pow(Mathf.Sin(t * Mathf.PI), thicknessFalloff);

            float thickness = Mathf.Lerp(edgeThickness, centerThickness, weight);

            vertices[i * 2] = center;
            vertices[i * 2 + 1] = center + normal * thickness;

            uvs[i * 2] = new Vector2(t, 1f);
            uvs[i * 2 + 1] = new Vector2(t, 0f);
        }

        for (int i = 0; i < segments; i++) {
            int bi = i * 6;
            int vi = i * 2;

            triangles[bi + 0] = vi;
            triangles[bi + 1] = vi + 2;
            triangles[bi + 2] = vi + 1;

            triangles[bi + 3] = vi + 1;
            triangles[bi + 4] = vi + 2;
            triangles[bi + 5] = vi + 3;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        ApplyMaterial();
    }

    void ApplyMaterial() {
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) return;

        if (lipMaterial != null) {
            // Assign the provided material and tint it
            meshRenderer.sharedMaterial = lipMaterial;
            lipMaterial.color = lipColor;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.TransformPoint(leftPoint), 0.04f);
        Gizmos.DrawSphere(transform.TransformPoint(rightPoint), 0.04f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(topPoint), 0.04f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.TransformPoint(leftPoint), transform.TransformPoint(topPoint));
        Gizmos.DrawLine(transform.TransformPoint(topPoint), transform.TransformPoint(rightPoint));

        Gizmos.color = Color.green;
        Vector3 prev = transform.TransformPoint(Bezier(0f));
        for (int i = 1; i <= segments; i++) {
            float t = (float)i / segments;
            Vector3 next = transform.TransformPoint(Bezier(t));
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LipMeshGenerator))]
public class LipMeshGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        LipMeshGenerator gen = (LipMeshGenerator)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Regenerate Mesh"))
            gen.GenerateMesh();

        if (GUILayout.Button("Reset Vertex Weights to Default")) {
            gen.vertexWeights = null;
            gen.OnValidate();
            gen.GenerateMesh();
        }
    }

    void OnSceneGUI() {
        LipMeshGenerator gen = (LipMeshGenerator)target;
        Transform t = gen.transform;

        EditorGUI.BeginChangeCheck();

        Vector3 newLeft = Handles.PositionHandle(t.TransformPoint(gen.leftPoint), Quaternion.identity);
        Vector3 newTop = Handles.PositionHandle(t.TransformPoint(gen.topPoint), Quaternion.identity);
        Vector3 newRight = Handles.PositionHandle(t.TransformPoint(gen.rightPoint), Quaternion.identity);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(gen, "Move Lip Control Point");
            gen.leftPoint = t.InverseTransformPoint(newLeft);
            gen.topPoint = t.InverseTransformPoint(newTop);
            gen.rightPoint = t.InverseTransformPoint(newRight);
            gen.GenerateMesh();
        }
    }
}
#endif