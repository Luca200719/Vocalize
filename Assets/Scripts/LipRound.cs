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

    [Header("Tongue Width (optional)")]
    [Tooltip("Leave empty to skip tongue width scaling")]
    public TongueMeshGenerator tongueGenerator;

    [Tooltip("Tongue half-width when lip is at minimum spread (narrow)")]
    public float tongueHalfWidthMin = 0.1f;

    [Tooltip("Tongue half-width when lip is at maximum spread (wide)")]
    public float tongueHalfWidthMax = 0.4f;

    private Vector3 centreLocal;
    private Vector3 spreadDir;
    private Vector3 tongueCentreLocal;
    private Vector3 tongueSpreadDir;

    void Start() {
        if (slider == null || lipGenerator == null) return;

        centreLocal = (lipGenerator.leftPoint + lipGenerator.rightPoint) * 0.5f;
        Vector3 toRight = lipGenerator.rightPoint - centreLocal;
        spreadDir = toRight.normalized;

        float currentSpread = toRight.magnitude;
        float t = Mathf.InverseLerp(minSpread, maxSpread, currentSpread);
        slider.value = Mathf.Clamp01(t);

        // Cache tongue spread direction too
        if (tongueGenerator != null) {
            tongueCentreLocal = (tongueGenerator.baseLeft + tongueGenerator.baseRight) * 0.5f;
            tongueSpreadDir = (tongueGenerator.baseRight - tongueCentreLocal).normalized;
        }

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value) {
        // Lip spreads wider as value increases
        float spread = Mathf.Lerp(minSpread, maxSpread, value);
        lipGenerator.leftPoint = centreLocal - spreadDir * spread;
        lipGenerator.rightPoint = centreLocal + spreadDir * spread;
        lipGenerator.GenerateMesh();

        if (tongueGenerator != null) {
            // Tongue gets narrower as lip gets wider — invert the value
            float tongueHalfWidth = Mathf.Lerp(tongueHalfWidthMax, tongueHalfWidthMin, value);
            tongueGenerator.baseLeft = tongueCentreLocal - tongueSpreadDir * tongueHalfWidth;
            tongueGenerator.baseRight = tongueCentreLocal + tongueSpreadDir * tongueHalfWidth;
            tongueGenerator.GenerateMesh();
        }
    }

    void OnDestroy() {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}