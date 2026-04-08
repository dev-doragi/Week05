using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraAreaController : MonoBehaviour
{
    [Header("Camera Data")]
    [SerializeField] private Texture _roomBackgroundTexture;
    [SerializeField] private Texture _roomBackgroundWithCoachTexture;
    [SerializeField] private bool _hasCoach;

    [Header("UI")]
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private Image _cameraUiBackground;
    [SerializeField] private Color _normalColor = Color.black;
    [SerializeField] private Color _blinkColor = Color.white;
    [SerializeField] private float _blinkInterval = 0.5f;

    private Coroutine _blinkCoroutine;

    private void OnValidate()
    {
        if (_cameraManager != null)
            _cameraManager.RefreshSelectedCamera();
    }

    public Texture CurrentBackgroundTexture
    {
        get
        {
            if (_hasCoach && _roomBackgroundWithCoachTexture != null)
                return _roomBackgroundWithCoachTexture;

            return _roomBackgroundTexture;
        }
    }

    public bool HasCoach => _hasCoach;

    public void OnClickCameraArea()
    {
        if (_cameraManager == null)
            return;

        _cameraManager.SelectCamera(this);
    }

    public void SetCoachPresence(bool hasCoach)
    {
        _hasCoach = hasCoach;

        if (_cameraManager != null)
            _cameraManager.RefreshSelectedCamera();
    }

    public void StartBlinking()
    {
        StopBlinking();
        _blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    public void StopBlinking()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        if (_cameraUiBackground != null)
            _cameraUiBackground.color = _normalColor;
    }

    private IEnumerator BlinkLoop()
    {
        if (_cameraUiBackground == null)
            yield break;

        bool isBlinkColor = false;

        while (true)
        {
            _cameraUiBackground.color = isBlinkColor ? _blinkColor : _normalColor;
            isBlinkColor = !isBlinkColor;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }
}