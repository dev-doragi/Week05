using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HierarchyComponentView : MonoBehaviour
{
    [SerializeField] private TMP_Text _componentNameText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _errorBlinkImage;

    [SerializeField][Range(0f, 1f)] private float _blinkMinAlpha = 0.10f;
    [SerializeField][Range(0f, 1f)] private float _blinkMaxAlpha = 0.45f;
    [SerializeField] private float _blinkSpeed = 4f;

    private bool _isErrorBlinking;

    public void Render(string name, Sprite iconSprite)
    {
        if (_componentNameText != null)
            _componentNameText.text = name;

        if (_iconImage != null)
            _iconImage.sprite = iconSprite;
    }

    public void SetErrorBlink(bool isBlinking)
    {
        _isErrorBlinking = isBlinking;

        if (_isErrorBlinking)
        {
            SetBlinkAlpha(_blinkMaxAlpha);
            return;
        }

        SetBlinkAlpha(0f);
    }

    private void OnDisable()
    {
        SetErrorBlink(false);
    }

    private void Update()
    {
        if (_isErrorBlinking == false || _errorBlinkImage == null)
            return;

        float t = (Mathf.Sin(Time.unscaledTime * _blinkSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(_blinkMinAlpha, _blinkMaxAlpha, t);
        SetBlinkAlpha(alpha);
    }

    private void SetBlinkAlpha(float alpha)
    {
        if (_errorBlinkImage == null)
            return;

        var color = _errorBlinkImage.color;
        color.a = alpha;
        _errorBlinkImage.color = color;
        _errorBlinkImage.enabled = alpha > 0f;
    }
}
