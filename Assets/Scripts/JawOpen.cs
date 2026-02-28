using UnityEngine;
using UnityEngine.UI;

public class JawOpen : MonoBehaviour {
    [Tooltip("Reference to the LipMeshGenerator component")]
    public LipMeshGenerator lipGenerator;

    [Tooltip("Slider normalized 0-1")]
    public Slider slider;

    [Header("Y Position Range")]
    public float minY = 0f;
    public float maxY = 1f;

    [Header("Thickness Range (optional)")]
    [Tooltip("Leave both at 0 to skip thickness animation")]
    public float minThickness = 0f;
    public float maxThickness = 0f;

    [Header("Tongue (optional)")]
    [Tooltip("Leave empty to skip tongue control")]
    public TongueMeshGenerator tongueGenerator;

    public float tongueTipYClosed = 0.1f;
    public float tongueTipYOpen = -0.3f;
    public float tongueBulgeClosed = 0.1f;
    public float tongueBulgeOpen = 0.5f;

    void Start() {
        if (slider == null || lipGenerator == null) return;

        float t = Mathf.InverseLerp(minY, maxY, lipGenerator.topPoint.y);
        slider.value = Mathf.Clamp01(t);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value) {
        if (lipGenerator == null) return;

        // Move lip top point Y
        Vector3 top = lipGenerator.topPoint;
        top.y = Mathf.Lerp(minY, maxY, value);
        lipGenerator.topPoint = top;

        // Only animate thickness if a range has been set
        if (minThickness != 0f || maxThickness != 0f)
            lipGenerator.centerThickness = Mathf.Lerp(minThickness, maxThickness, value);

        lipGenerator.GenerateMesh();

        // Only drive tongue if a generator has been assigned
        if (tongueGenerator != null) {
            Vector3 tip = tongueGenerator.tipPoint;
            tip.y = Mathf.Lerp(tongueTipYClosed, tongueTipYOpen, value);
            tongueGenerator.tipPoint = tip;

            tongueGenerator.bulge = Mathf.Lerp(tongueBulgeClosed, tongueBulgeOpen, value);
            tongueGenerator.GenerateMesh();
        }
    }

    void OnDestroy() {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}