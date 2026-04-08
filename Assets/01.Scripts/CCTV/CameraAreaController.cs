using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraAreaController : MonoBehaviour
{
    [Header("Room Data")]
    [SerializeField] private RoomID _roomId = RoomID.None;
    [SerializeField] private Texture _roomBackgroundTexture;
    [SerializeField] private Texture _roomBackgroundWithCoachTexture;

    [Header("UI")]
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private Image _cameraUiBackground;
    [SerializeField] private Color _normalColor = Color.black;
    [SerializeField] private Color _blinkColor = Color.white;
    [SerializeField] private float _blinkInterval = 0.5f;

    private Coroutine _blinkCoroutine;

    public RoomID RoomId => _roomId;

    public Texture GetBackgroundTexture(bool hasCoach)
    {
        if (hasCoach && _roomBackgroundWithCoachTexture != null)
            return _roomBackgroundWithCoachTexture;

        return _roomBackgroundTexture;
    }

    public void OnClickCameraArea()
    {
        if (_cameraManager == null)
            return;

        _cameraManager.SelectCamera(this);
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