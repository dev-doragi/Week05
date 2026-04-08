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
    [SerializeField] private CoachMovementController _coachMovementController;
    [SerializeField] private Image _cameraUiBackground;
    [SerializeField] private Color _normalColor = Color.black;
    [SerializeField] private Color _blinkColor = Color.yellow;
    [SerializeField] private Color _coachDetectionColor = Color.red; // 코치 위치 표시 색상
    [SerializeField] private float _blinkInterval = 0.5f;

    private Coroutine _blinkCoroutine;
    private bool _isSelected;
    private static bool _isHardMode; // 하드 모드 상태 (전역)

    public RoomID RoomId => _roomId;

    private void Awake()
    {
        if (_coachMovementController == null)
        {
            _coachMovementController = Object.FindAnyObjectByType<CoachMovementController>();
        }
    }

    private void Update()
    {
        bool isCoachHere = _coachMovementController != null && _coachMovementController.IsCoachInRoom(_roomId);

        // 로직 변경: (내가 선택되었거나) OR (하드 모드가 아니면서 코치가 이 방에 있거나)
        bool shouldBlink = _isSelected || (!_isHardMode && isCoachHere);

        if (shouldBlink)
        {
            if (_blinkCoroutine == null)
                _blinkCoroutine = StartCoroutine(BlinkLoop());
        }
        else
        {
            // 깜빡일 조건이 아니면 루틴 정지 및 색상 초기화
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

    public void OnClickCameraArea()
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
            // 우선순위: 선택 상태(노란색) > 코치 탐지 상태(붉은색)
            Color targetColor = _isSelected ? _blinkColor : _coachDetectionColor;

            _cameraUiBackground.color = isBlinkColor ? targetColor : _normalColor;
            isBlinkColor = !isBlinkColor;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }

    // 하드 모드 설정 (토글 UI와 연결)
    public static void SetHardMode(bool active)
    {
        _isHardMode = active;
    }
}