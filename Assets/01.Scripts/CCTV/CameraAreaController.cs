using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraAreaController : MonoBehaviour, IPointerClickHandler
{
    public enum BlinkState
    {
        None,
        Selected,
        CoachDetected
    }

    [Header("Room Data")]
    [SerializeField] private RoomID _roomId = RoomID.None;
    [SerializeField] private Texture _roomBackgroundTexture;
    [SerializeField] private Texture _roomBackgroundWithCoachTexture;

    [Header("UI")]
    [SerializeField] private Image _cameraUiBackground;
    [SerializeField] private Color _normalColor = Color.black;
    [SerializeField] private Color _selectedBlinkColor = Color.yellow;
    [SerializeField] private Color _coachDetectedBlinkColor = Color.red;
    [SerializeField] private float _blinkInterval = 0.5f;

    private CameraManager _cameraManager;
    private Coroutine _blinkCoroutine;
    private BlinkState _currentBlinkState = BlinkState.None;

    public RoomID RoomId => _roomId;

    private void Awake()
    {
        _cameraManager = FindAnyObjectByType<CameraManager>();
    }

    public Texture GetBackgroundTexture(bool hasCoach)
    {
        if (hasCoach && _roomBackgroundWithCoachTexture != null)
            return _roomBackgroundWithCoachTexture;

        return _roomBackgroundTexture;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cameraManager == null)
            return;

        _cameraManager.SelectCamera(this);
    }

    public void SetBlinkState(BlinkState blinkState)
    {
        if (_currentBlinkState == blinkState)
            return;

        _currentBlinkState = blinkState;

        if (_currentBlinkState == BlinkState.None)
        {
            StopBlinkingInternal();
            return;
        }

        if (_blinkCoroutine == null)
            _blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    private void StopBlinkingInternal()
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
            _cameraUiBackground.color = isBlinkColor ? GetBlinkColor() : _normalColor;
            isBlinkColor = !isBlinkColor;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }

    private Color GetBlinkColor()
    {
        return _currentBlinkState switch
        {
            BlinkState.Selected => _selectedBlinkColor,
            BlinkState.CoachDetected => _coachDetectedBlinkColor,
            _ => _normalColor
        };
    }
}