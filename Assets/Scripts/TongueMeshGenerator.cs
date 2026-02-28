using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TongueMeshGenerator : MonoBehaviour {
    [Header("Control Points")]
    [Tooltip("Base left point (root of tongue)")]
    public Vector3 baseLeft = new Vector3(-0.5f, 0f, 0f);

    [Tooltip("Base right point (root of tongue)")]
    public Vector3 baseRight = new Vector3(0.5f, 0f, 0f);

    [Tooltip("Tip point (controlled by slider)")]
    public Vector3 tipPoint = new Vector3(0f, 0.6f, 0f);

    [Header("Shape")]
    [Tooltip("How much the sides bow outward. Higher = fatter/rounder tongue.")]
    [Range(0f, 1f)]
    public float bulge = 0.4f;

    [Tooltip("Sharpness of the tip taper (higher = pointer tip)")]
    [Range(0.5f, 4f)]
    public float tipTaper = 1.5f;

    [Header("Material")]
    public Material tongueMaterial;
    public Color tongueColor = new Color(0.85f, 0.4f, 0.4f, 1f);

    [Header("Resolution")]
    [Range(8, 128)]
    public int segments = 48;

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Awake() => GenerateMesh();

    void OnValidate() {
        if (Application.isPlaying)
            GenerateMesh();
    }

    // The spine runs from baseMid up to tipPoint
    // We use a straight line but bow the LEFT and RIGHT edges outward using offset Beziers

    // Left edge: baseLeft -> bulged left control -> tipPoint
    Vector3 LeftEdge(float t) {
        Vector3 baseMid = (baseLeft + baseRight) * 0.5f;
        // Control point bows outward from the left
        Vector3 ctrl = baseLeft + (tipPoint - baseMid) * 0.5f + Vector3.left * bulge * Vector3.Distance(baseLeft, baseRight) * 0.5f;
        float u = 1f - t;
        return u * u * baseLeft + 2f * u * t * ctrl + t * t * tipPoint;
    }

    // Right edge: baseRight -> bulged right control -> tipPoint
    Vector3 RightEdge(float t) {
        Vector3 baseMid = (baseLeft + baseRight) * 0.5f;
        Vector3 ctrl = baseRight + (tipPoint - baseMid) * 0.5f + Vector3.right * bulge * Vector3.Distance(baseLeft, baseRight) * 0.5f;
        float u = 1f - t;
        return u * u * baseRight + 2f * u * t * ctrl + t * t * tipPoint;
    }

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh() {
        meshFilter = GetComponent<MeshFilter>();
        if (mesh == null) {
            mesh = new Mesh();
            mesh.name = "TongueMesh";
        }
        mesh.Clear();

        // Vertices: for each segment, a left and right point on their respective Bezier edges
        // At t=1 (tip) both edges meet at tipPoint
        int vertCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++) {
            float t = (float)i / segments;

            // Smooth taper: at t=1 the two edges fully converge at the tip
            // Use a power curve so the tip rounds off rather than pinching
            float taper = Mathf.Pow(1f - t, tipTaper);

            Vector3 left = LeftEdge(t);
            Vector3 right = RightEdge(t);

            // Lerp edges toward tip as taper closes
            left = Vector3.Lerp(tipPoint, left, 1f - taper * 0f); // edges already converge via Bezier
            right = Vector3.Lerp(tipPoint, right, 1f - taper * 0f);

            vertices[i * 2] = left;
            vertices[i * 2 + 1] = right;

            uvs[i * 2] = new Vector2(0f, t);
            uvs[i * 2 + 1] = new Vector2(1f, t);
        }

        // Add a rounded cap at the tip by blending the last few verts to the tip center
        int tipStart = Mathf.Max(0, segments - (segments / 4));
        for (int i = tipStart; i <= segments; i++) {
            float localT = (float)(i - tipStart) / (segments - tipStart);
            float blend = Mathf.SmoothStep(0f, 1f, localT);

            vertices[i * 2] = Vector3.Lerp(vertices[i * 2], tipPoint, blend);
            vertices[i * 2 + 1] = Vector3.Lerp(vertices[i * 2 + 1], tipPoint, blend);
        }

        for (int i = 0; i < segments; i++) {
            int bi = i * 6;
            int vi = i * 2;

            triangles[bi + 0] = vi;
            triangles[bi + 1] = vi + 1;
            triangles[bi + 2] = vi + 2;

            triangles[bi + 3] = vi + 1;
            triangles[bi + 4] = vi + 3;
            triangles[bi + 5] = vi + 2;
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
        var r = GetComponent<MeshRenderer>();
        if (r == null || tongueMaterial == null) return;
        r.sharedMaterial = tongueMaterial;
        tongueMaterial.color = tongueColor;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.TransformPoint(baseLeft), 0.04f);
        Gizmos.DrawSphere(transform.TransformPoint(baseRight), 0.04f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(tipPoint), 0.04f);

        // Draw the two bezier edges
        Gizmos.color = Color.green;
        Vector3 prevL = transform.TransformPoint(LeftEdge(0f));
        Vector3 prevR = transform.TransformPoint(RightEdge(0f));
        for (int i = 1; i <= segments; i++) {
            float t = (float)i / segments;
            Vector3 nextL = transform.TransformPoint(LeftEdge(t));
            Vector3 nextR = transform.TransformPoint(RightEdge(t));
            Gizmos.DrawLine(prevL, nextL);
            Gizmos.DrawLine(prevR, nextR);
            prevL = nextL;
            prevR = nextR;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TongueMeshGenerator))]
public class TongueMeshGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        if (GUILayout.Button("Regenerate Mesh"))
            ((TongueMeshGenerator)target).GenerateMesh();
    }

    void OnSceneGUI() {
        TongueMeshGenerator gen = (TongueMeshGenerator)target;
        Transform t = gen.transform;

        EditorGUI.BeginChangeCheck();

        Vector3 newBaseLeft = Handles.PositionHandle(t.TransformPoint(gen.baseLeft), Quaternion.identity);
        Vector3 newBaseRight = Handles.PositionHandle(t.TransformPoint(gen.baseRight), Quaternion.identity);
        Vector3 newTip = Handles.PositionHandle(t.TransformPoint(gen.tipPoint), Quaternion.identity);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(gen, "Move Tongue Control Point");
            gen.baseLeft = t.InverseTransformPoint(newBaseLeft);
            gen.baseRight = t.InverseTransformPoint(newBaseRight);
            gen.tipPoint = t.InverseTransformPoint(newTip);
            gen.GenerateMesh();
        }
    }
}
#endif