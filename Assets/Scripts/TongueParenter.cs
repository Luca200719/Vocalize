using UnityEngine;

/// <summary>
/// Anchors the tongue BASE to the spline midpoint of the lower lip over time.
/// Only controls baseLeft/baseRight and Z — tip Y is left to TongueTipSlider.
/// </summary>
public class TongueParenter : MonoBehaviour {
    [Tooltip("The lower lip to attach the tongue to")]
    public LipMeshGenerator lowerLip;

    [Tooltip("The tongue mesh to reposition")]
    public TongueMeshGenerator tongue;

    [Tooltip("Half-width of the tongue base when lip is fully open")]
    public float tongueHalfWidth = 0.4f;

    [Tooltip("Z offset to push tongue behind the lips (negative = further back)")]
    public float zOffset = -0.1f;

    [Tooltip("How quickly the tongue base follows the lip (higher = snappier)")]
    public float followSpeed = 10f;

    private Vector3 currentBaseLeft;
    private Vector3 currentBaseRight;
    private bool initialised = false;

    Vector3 LipSplineMidpoint() {
        Vector3 p0 = lowerLip.leftPoint;
        Vector3 p1 = lowerLip.topPoint;
        Vector3 p2 = lowerLip.rightPoint;
        float t = 0.5f, u = 0.5f;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    float LipOpenAmount() {
        float baseY = (lowerLip.leftPoint.y + lowerLip.rightPoint.y) * 0.5f;
        return Mathf.Clamp01(Mathf.Abs(lowerLip.topPoint.y - baseY));
    }

    void Update() {
        if (lowerLip == null || tongue == null) return;

        Vector3 splineMidWorld = lowerLip.transform.TransformPoint(LipSplineMidpoint());
        Vector3 anchor = tongue.transform.InverseTransformPoint(splineMidWorld);
        anchor.z += zOffset;

        float halfWidth = tongueHalfWidth * LipOpenAmount();
        Vector3 targetLeft = anchor + Vector3.left * halfWidth;
        Vector3 targetRight = anchor + Vector3.right * halfWidth;

        // Snap on first frame, smooth thereafter
        if (!initialised) {
            currentBaseLeft = targetLeft;
            currentBaseRight = targetRight;
            initialised = true;
        }
        else {
            currentBaseLeft = Vector3.Lerp(currentBaseLeft, targetLeft, followSpeed * Time.deltaTime);
            currentBaseRight = Vector3.Lerp(currentBaseRight, targetRight, followSpeed * Time.deltaTime);
        }

        tongue.baseLeft = currentBaseLeft;
        tongue.baseRight = currentBaseRight;

        // Only update tip X and Z — leave tip Y to the slider
        Vector3 tip = tongue.tipPoint;
        tip.x = anchor.x;
        tip.z = anchor.z;
        tongue.tipPoint = tip;

        tongue.GenerateMesh();
    }
}