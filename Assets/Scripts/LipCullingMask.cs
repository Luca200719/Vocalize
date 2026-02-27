using UnityEngine;
public class LipCullingCamera : MonoBehaviour {
    [Tooltip("The layer your lip meshes are assigned to")]
    public string lipLayerName = "Lip";

    [Tooltip("The background camera that renders everything behind the lips")]
    public Camera backgroundCamera;

    private Camera cam;

    void Awake() {
        cam = GetComponent<Camera>();
        Apply();
    }

    void OnValidate() {
        if (cam == null) cam = GetComponent<Camera>();
        Apply();
    }

    void Apply() {
        if (cam == null) return;

        int layer = LayerMask.NameToLayer(lipLayerName);
        if (layer == -1) {
            Debug.LogWarning($"LipCullingCamera: Layer '{lipLayerName}' not found. Please create it in Project Settings > Tags and Layers.");
            return;
        }

        // This camera ONLY renders the lip layer
        cam.cullingMask = 1 << layer;
        cam.clearFlags = CameraClearFlags.Depth;

        // Match background camera's projection so they line up perfectly
        if (backgroundCamera != null) {
            cam.fieldOfView = backgroundCamera.fieldOfView;
            cam.orthographic = backgroundCamera.orthographic;
            cam.orthographicSize = backgroundCamera.orthographicSize;
            cam.nearClipPlane = backgroundCamera.nearClipPlane;
            cam.farClipPlane = backgroundCamera.farClipPlane;
            cam.depth = backgroundCamera.depth + 1;
        }
    }
}