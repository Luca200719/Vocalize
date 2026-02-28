using UnityEngine;
using UnityEngine.UI;

public class TongueHeight : MonoBehaviour {
    [Tooltip("Reference to the TongueMeshGenerator component")]
    public TongueMeshGenerator tongueGenerator;

    [Tooltip("Normalized 0-1 slider")]
    public Slider slider;

    [Header("Y Range")]
    public float minY = 0f;
    public float maxY = 1f;

    [Header("Bulge Range")]
    public float minBulge = 0f;
    public float maxBulge = 0.6f;

    void Start() {
        if (slider == null || tongueGenerator == null) return;

        float t = Mathf.InverseLerp(minY, maxY, tongueGenerator.tipPoint.y);
        slider.value = Mathf.Clamp01(t);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value) {
        Vector3 tip = tongueGenerator.tipPoint;
        tip.y = Mathf.Lerp(minY, maxY, value);
        tongueGenerator.tipPoint = tip;

        tongueGenerator.bulge = Mathf.Lerp(minBulge, maxBulge, value);

        tongueGenerator.GenerateMesh();
    }

    /// <summary>
    /// Re-applies the current slider value using the latest minY/maxY.
    /// Call this when the range has been changed externally (e.g. by LipTopPointSlider).
    /// </summary>
    public void ForceUpdate() {
        if (slider != null)
            OnSliderChanged(slider.value);
    }

    void OnDestroy() {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}