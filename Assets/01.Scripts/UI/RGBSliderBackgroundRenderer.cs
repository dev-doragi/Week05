using UnityEngine;
using UnityEngine.UI;

public class RGBSliderBackgroundRenderer : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _redSlider;
    [SerializeField] private Slider _greenSlider;
    [SerializeField] private Slider _blueSlider;

    [Header("Slider Backgrounds (RawImage)")]
    [SerializeField] private RawImage _redBackground;
    [SerializeField] private RawImage _greenBackground;
    [SerializeField] private RawImage _blueBackground;

    [Header("Texture Settings")]
    [SerializeField][Min(2)] private int _textureWidth = 256;

    private Texture2D _redTexture;
    private Texture2D _greenTexture;
    private Texture2D _blueTexture;

    private void Awake()
    {
        if (HasMissingReference())
        {
            Debug.LogWarning("[RGBSliderBackgroundRenderer] 연결되지 않은 참조가 있습니다.");
            enabled = false;
            return;
        }

        CreateTextures();
        ConnectSliderEvents();
        RefreshAllBackgrounds();
    }

    private void OnEnable()
    {
        RefreshAllBackgrounds();
    }

    private void OnDestroy()
    {
        DisconnectSliderEvents();
        DestroyTextures();
    }

    private bool HasMissingReference()
    {
        return _redSlider == null
            || _greenSlider == null
            || _blueSlider == null
            || _redBackground == null
            || _greenBackground == null
            || _blueBackground == null;
    }

    private void CreateTextures()
    {
        _redTexture = CreateSingleLineTexture();
        _greenTexture = CreateSingleLineTexture();
        _blueTexture = CreateSingleLineTexture();

        _redBackground.texture = _redTexture;
        _greenBackground.texture = _greenTexture;
        _blueBackground.texture = _blueTexture;
    }

    private Texture2D CreateSingleLineTexture()
    {
        Texture2D texture = new Texture2D(_textureWidth, 1, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    private void ConnectSliderEvents()
    {
        _redSlider.onValueChanged.AddListener(OnAnySliderValueChanged);
        _greenSlider.onValueChanged.AddListener(OnAnySliderValueChanged);
        _blueSlider.onValueChanged.AddListener(OnAnySliderValueChanged);
    }

    private void DisconnectSliderEvents()
    {
        if (_redSlider != null)
            _redSlider.onValueChanged.RemoveListener(OnAnySliderValueChanged);

        if (_greenSlider != null)
            _greenSlider.onValueChanged.RemoveListener(OnAnySliderValueChanged);

        if (_blueSlider != null)
            _blueSlider.onValueChanged.RemoveListener(OnAnySliderValueChanged);
    }

    private void OnAnySliderValueChanged(float _)
    {
        RefreshAllBackgrounds();
    }

    public void RefreshAllBackgrounds()
    {
        if (!isActiveAndEnabled)
            return;

        if (_redTexture == null || _greenTexture == null || _blueTexture == null)
            return;

        float red = GetSliderValue01(_redSlider);
        float green = GetSliderValue01(_greenSlider);
        float blue = GetSliderValue01(_blueSlider);

        DrawRedSliderBackground(green, blue);
        DrawGreenSliderBackground(red, blue);
        DrawBlueSliderBackground(red, green);
    }

    private float GetSliderValue01(Slider slider)
    {
        return Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
    }

    private void DrawRedSliderBackground(float currentGreen, float currentBlue)
    {
        for (int x = 0; x < _textureWidth; x++)
        {
            float red = x / (float)(_textureWidth - 1);
            Color color = new Color(red, currentGreen, currentBlue);
            _redTexture.SetPixel(x, 0, color);
        }

        _redTexture.Apply();
    }

    private void DrawGreenSliderBackground(float currentRed, float currentBlue)
    {
        for (int x = 0; x < _textureWidth; x++)
        {
            float green = x / (float)(_textureWidth - 1);
            Color color = new Color(currentRed, green, currentBlue);
            _greenTexture.SetPixel(x, 0, color);
        }

        _greenTexture.Apply();
    }

    private void DrawBlueSliderBackground(float currentRed, float currentGreen)
    {
        for (int x = 0; x < _textureWidth; x++)
        {
            float blue = x / (float)(_textureWidth - 1);
            Color color = new Color(currentRed, currentGreen, blue);
            _blueTexture.SetPixel(x, 0, color);
        }

        _blueTexture.Apply();
    }

    private void DestroyTextures()
    {
        DestroyTexture(_redTexture);
        DestroyTexture(_greenTexture);
        DestroyTexture(_blueTexture);
    }

    private void DestroyTexture(Texture2D texture)
    {
        if (texture != null)
            Destroy(texture);
    }
}