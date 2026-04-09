using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // IPointerClickHandler 사용을 위해 추가

public class CameraAreaController : MonoBehaviour, IPointerClickHandler
{
    [Header("Room Data")]
    [SerializeField] private RoomID _roomId = RoomID.None;
    [SerializeField] private Texture _roomBackgroundTexture;
    [SerializeField] private Texture _roomBackgroundWithCoachTexture;

    [Header("UI")]
    [SerializeField] private Image _cameraUiBackground;
    [SerializeField] private Color _normalColor = Color.black;
    [SerializeField] private Color _blinkColor = Color.yellow;
    [SerializeField] private Color _coachDetectionColor = Color.red;
    [SerializeField] private float _blinkInterval = 0.5f;

    private CameraManager _cameraManager;
    private CoachMovementController _coachMovementController;

    private Coroutine _blinkCoroutine;
    private bool _isSelected;
    private static bool _isHardMode;

    public RoomID RoomId => _roomId;

    private void Awake()
    {
        _cameraManager = FindAnyObjectByType<CameraManager>();
        _coachMovementController = FindAnyObjectByType<CoachMovementController>();
    }

    private void Update()
    {
        bool isCoachHere = _coachMovementController != null && _coachMovementController.IsCoachInRoom(_roomId);

        bool shouldBlink = _isSelected || (!_isHardMode && isCoachHere);

        if (shouldBlink)
        {
            if (_blinkCoroutine == null)
                _blinkCoroutine = StartCoroutine(BlinkLoop());
        }
        else
        {
            if (_blinkCoroutine != null)
            {
                StopBlinkingInternal();
            }
        }
    }

    public Texture GetBackgroundTexture(bool hasCoach)
    {
        if (hasCoach && _roomBackgroundWithCoachTexture != null)
            return _roomBackgroundWithCoachTexture;

        return _roomBackgroundTexture;
    }

    // 이벤트 트리거 -> UI 클릭 인터페이스로 변경; 하나 하나 할당하기 귀찮음
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_cameraManager == null)
            return;

        _cameraManager.SelectCamera(this);
    }

    public void StartBlinking()
    {
        _isSelected = true;
    }

    public void StopBlinking()
    {
        _isSelected = false;
        StopBlinkingInternal();
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
            Color targetColor = _isSelected ? _blinkColor : _coachDetectionColor;

            _cameraUiBackground.color = isBlinkColor ? targetColor : _normalColor;
            isBlinkColor = !isBlinkColor;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }

    public static void SetHardMode(bool active)
    {
        _isHardMode = active;
    }
}