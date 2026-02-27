using UnityEngine;
using UnityEngine.UI;

public class LipRound : MonoBehaviour {
    [Tooltip("Reference to the LipMeshGenerator component")]
    public LipMeshGenerator lipGenerator;

    [Tooltip("Slider normalized 0-1")]
    public Slider slider;

    [Header("Range")]
    public float minSpread = 0.2f;
    public float maxSpread = 3f;

    void Start() {
        if (slider == null || lipGenerator == null) return;

        // Map current lip spread back to 0-1 for the slider
        float currentSpread = Mathf.Abs(lipGenerator.rightPoint.x);
        float t = Mathf.InverseLerp(minSpread, maxSpread, currentSpread);
        slider.value = Mathf.Clamp01(t);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value) {
        // Map 0-1 slider to min/max spread range
        float spread = Mathf.Lerp(minSpread, maxSpread, value);

        Vector3 left = lipGenerator.leftPoint;
        Vector3 right = lipGenerator.rightPoint;

        left.x = -spread;
        right.x = spread;

        lipGenerator.leftPoint = left;
        lipGenerator.rightPoint = right;
        lipGenerator.GenerateMesh();
    }

    void OnDestroy() {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}