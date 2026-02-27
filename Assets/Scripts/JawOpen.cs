using UnityEngine;
using UnityEngine.UI;

public class LipTopPointSlider : MonoBehaviour {
    [Tooltip("Reference to the LipMeshGenerator component")]
    public LipMeshGenerator lipGenerator;

    [Tooltip("Slider normalized 0-1")]
    public Slider slider;

    [Header("Y Position Range")]
    public float minY = 0f;
    public float maxY = 1f;

    [Header("Thickness Range")]
    public float minThickness = 0.01f;
    public float maxThickness = 0.3f;

    void Start() {
        if (slider == null || lipGenerator == null) return;

        float t = Mathf.InverseLerp(minY, maxY, lipGenerator.topPoint.y);
        slider.value = Mathf.Clamp01(t);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value) {
        // Move top point Y
        Vector3 top = lipGenerator.topPoint;
        top.y = Mathf.Lerp(minY, maxY, value);
        lipGenerator.topPoint = top;

        // Animate thickness alongside it
        lipGenerator.centerThickness = Mathf.Lerp(minThickness, maxThickness, value);

        lipGenerator.GenerateMesh();
    }

    void OnDestroy() {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}