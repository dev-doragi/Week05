using UnityEngine;
using UnityEngine.UI;

public class RGBMatchGame : MiniGame
{
    [Header("References")]
    public SpriteRenderer sampleRect;
    public SpriteRenderer playerRect;
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public Button submitButton;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float clearThreshold = 0.1f;

    Color _targetColor;

    void Awake()
    {
        sliderR.onValueChanged.AddListener(_ => UpdatePlayerColor());
        sliderG.onValueChanged.AddListener(_ => UpdatePlayerColor());
        sliderB.onValueChanged.AddListener(_ => UpdatePlayerColor());
        submitButton.onClick.AddListener(OnSubmit);
    }

    protected override void OnStart()
    {
        _targetColor = new Color(Random.value, Random.value, Random.value);
        sampleRect.color = _targetColor;

        sliderR.value = 0f;
        sliderG.value = 0f;
        sliderB.value = 0f;

        UpdatePlayerColor();
    }

    void UpdatePlayerColor()
    {
        playerRect.color = new Color(sliderR.value, sliderG.value, sliderB.value);
    }

    void OnSubmit()
    {
        float diff = ColorDifference(playerRect.color, _targetColor);

        Debug.Log($"[RGBMatchGame] 오차: {diff:F3} / 기준: {clearThreshold:F3}");

        if (diff <= clearThreshold) Clear();
        else Debug.Log("[RGBMatchGame] 아직 멀었다 - 계속 조절");
    }

    float ColorDifference(Color a, Color b)
    {
        return (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b)) / 3f;
    }
}