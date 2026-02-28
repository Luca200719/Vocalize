using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MouthAI : MonoBehaviour {

    [Header("Sliders")]
    public Slider jawOpenSlider;
    public Slider lipRoundSlider;
    public Slider tongueHeightSlider;

    [Header("UI")]
    public TextMeshProUGUI feedbackText;
    public Button submitButton;
    public Button resetButton;

    // all target values are clamped to 0.1-1
    // jaw: 0.1 = almost closed, 1 = wide open
    // lip: 0.1 = spread, 1 = fully rounded
    // tongue: 0.1 = low, 1 = high
    private List<(string name, float jaw, float lip, float tongue)> sounds = new List<(string, float, float, float)> {
        ("TH", 0.3f, 0.1f, 0.3f),
        ("R",  0.4f, 0.6f, 0.5f),
        ("W",  0.3f, 0.9f, 0.5f),
        ("L",  0.4f, 0.1f, 0.9f),
        ("S",  0.2f, 0.1f, 0.7f),
        ("V",  0.3f, 0.1f, 0.4f)
    };

    private string currentTarget = "TH";
    private bool submitted = false;

    void Start() {
        jawOpenSlider.minValue = 0.1f; jawOpenSlider.maxValue = 1f;
        lipRoundSlider.minValue = 0.1f; lipRoundSlider.maxValue = 1f;
        tongueHeightSlider.minValue = 0.1f; tongueHeightSlider.maxValue = 1f;

        submitButton.onClick.AddListener(Submit);
        resetButton.onClick.AddListener(Reset);
        resetButton.gameObject.SetActive(false);
        feedbackText.text = $"Arrange your mouth to make the '{currentTarget}' sound";
    }

    void Update() {
        if (submitted) return;

        float jaw = Mathf.Clamp(jawOpenSlider.value, 0.1f, 1f);
        float lip = Mathf.Clamp(lipRoundSlider.value, 0.1f, 1f);
        float tongue = Mathf.Clamp(tongueHeightSlider.value, 0.1f, 1f);

        feedbackText.text = $"Target:   {currentTarget}\n" +
                            $"Detected: {Classify(jaw, lip, tongue)}\n" +
                            $"Match:    {Mathf.RoundToInt(GetScore(jaw, lip, tongue) * 100)}%\n\n" +
                            GetHint(jaw, lip, tongue);
    }

    void Submit() {
        submitted = true;

        float jaw = Mathf.Clamp(jawOpenSlider.value, 0.1f, 1f);
        float lip = Mathf.Clamp(lipRoundSlider.value, 0.1f, 1f);
        float tongue = Mathf.Clamp(tongueHeightSlider.value, 0.1f, 1f);

        string detected = Classify(jaw, lip, tongue);
        float score = GetScore(jaw, lip, tongue);

        jawOpenSlider.interactable = false;
        lipRoundSlider.interactable = false;
        tongueHeightSlider.interactable = false;

        string result;
        if (score > 0.8f)
            result = "✓ Excellent! That's correct!";
        else if (score > 0.5f)
            result = $"Almost! Your mouth looks more like '{detected}'.\n{GetHint(jaw, lip, tongue)}";
        else
            result = $"Not quite. Your mouth looks more like '{detected}'.\n{GetHint(jaw, lip, tongue)}";

        feedbackText.text = $"Target:   {currentTarget}\n" +
                            $"Detected: {detected}\n" +
                            $"Match:    {Mathf.RoundToInt(score * 100)}%\n\n" +
                            result;

        submitButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(true);
    }

    void Reset() {
        submitted = false;

        jawOpenSlider.value = 1f;
        lipRoundSlider.value = 1f;
        tongueHeightSlider.value = 1f;


        jawOpenSlider.interactable = true;
        lipRoundSlider.interactable = true;
        tongueHeightSlider.interactable = true;

        submitButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(false);

        feedbackText.text = $"Arrange your mouth to make the '{currentTarget}' sound";
    }

    string Classify(float jaw, float lip, float tongue) {
        return sounds
            .OrderBy(s => Distance(jaw, lip, tongue, s.jaw, s.lip, s.tongue))
            .First().name;
    }

    float GetScore(float jaw, float lip, float tongue) {
        var target = sounds.First(s => s.name == currentTarget);
        float dist = Distance(jaw, lip, tongue, target.jaw, target.lip, target.tongue);
        // max possible distance in a 0.1-1 cube is sqrt(3 * 0.9^2) ≈ 1.559
        return 1f - Mathf.Clamp01(dist / Mathf.Sqrt(3f * 0.9f * 0.9f));
    }

    float Distance(float j1, float l1, float t1, float j2, float l2, float t2) {
        return Mathf.Sqrt(
            Mathf.Pow(j1 - j2, 2) +
            Mathf.Pow(l1 - l2, 2) +
            Mathf.Pow(t1 - t2, 2)
        );
    }

    string GetHint(float jaw, float lip, float tongue) {
        var target = sounds.First(s => s.name == currentTarget);

        float dJaw = Mathf.Abs(jaw - target.jaw);
        float dLip = Mathf.Abs(lip - target.lip);
        float dTongue = Mathf.Abs(tongue - target.tongue);

        float max = Mathf.Max(dJaw, dLip, dTongue);

        if (max == dJaw)
            return jaw < target.jaw ? "→ Open your jaw more" : "→ Close your jaw a little";
        if (max == dLip)
            return lip < target.lip ? "→ Round your lips more" : "→ Spread your lips more";
        if (max == dTongue)
            return tongue < target.tongue ? "→ Raise your tongue" : "→ Lower your tongue";

        return "✓ Looking good!";
    }
}