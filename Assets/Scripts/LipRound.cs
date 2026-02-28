using UnityEngine;
using UnityEngine.UI;

public class LipWidthSlider : MonoBehaviour {
    [Tooltip("Reference to the LipMeshGenerator component")]
    public LipMeshGenerator lipGenerator;

    [Tooltip("Slider normalized 0-1")]
    public Slider slider;

    [Header("Range")]
    [Tooltip("Spread at slider 0 (narrow)")]
    public float minSpread = 0.2f;

    [Tooltip("Spread at slider 1 (wide)")]
    public float maxSpread = 3f;

    [Header("Tongue Y Scale (optional)")]
    [Tooltip("Leave empty to skip tongue Y scaling")]
    public TongueMeshGenerator tongueGenerator;

    [Tooltip("Tongue tip Y when lip is at minimum spread")]
    public float tongueTipYMin = 0.1f;

    [Tooltip("Tongue tip Y when lip is at maximum spread")]
    public float tongueTipYMax = 0.5f;

    // Base X centre is the midpoint of left and right — spread outward from here
    float BaseCentreX() => (lipGenerator.leftPoint.x + lipGenerator.rightPoint.x) * 0.5f;

    void Start() {
        if (slider == null || lipGenerator == null) return;

        float currentSpread = Mathf.Abs(lipGenerator.rightPoint.x - BaseCentreX());
        float t = Mathf.InverseLerp(minSpread, maxSpread, currentSpread);
        slider.value = Mathf.Clamp01(t);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value) {
        float spread = Mathf.Lerp(minSpread, maxSpread, value);
        float centre = BaseCentreX();

        Vector3 left = lipGenerator.leftPoint;
        Vector3 right = lipGenerator.rightPoint;

        // Spread outward from centre, not from zero
        left.x = centre - spread;
        right.x = centre + spread;

        lipGenerator.leftPoint = left;
        lipGenerator.rightPoint = right;
        lipGenerator.GenerateMesh();

        if (tongueGenerator != null) {
            Vector3 tip = tongueGenerator.tipPoint;
            tip.y = Mathf.Lerp(tongueTipYMin, tongueTipYMax, value);
            tongueGenerator.tipPoint = tip;
            tongueGenerator.GenerateMesh();
        }
    }

    void OnDestroy() {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}