using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ToastItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image _profileImage;
    [SerializeField] private Outline _toastOutline;
    [SerializeField] private TextMeshProUGUI _workspaceNameText;
    [SerializeField] private TextMeshProUGUI _senderNameText;
    [SerializeField] private TextMeshProUGUI _messageContextText;

    [Header("Animation Settings")]
    [SerializeField] private float _hiddenX = 0f;
    [SerializeField] private float _visibleX = 0f;
    [SerializeField] private float _enterDuration = 0.5f;
    [SerializeField] private float _exitDuration = 0.5f;

    [SerializeField] private float _swipeCloseThreshold = 150f;
    [SerializeField] private float _swipeReturnDuration = 0.2f;

    private bool _isDragging;
    private bool _isSwiped;
    private Vector2 _dragStartPointerPosition;
    private float _dragStartAnchoredX;

    [SerializeField] private Button _bodyButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private bool _focusCoachRoomOnOpen = true;

    private CoachMovementController _coachMovementController;
    private UI_ToastSender _owner;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Tween _stackMoveTween;
    private Sequence _sequence;
    private float _baseAnchoredY;
    private bool _isClosing;

    public float Height => _rectTransform.rect.height;
    public float BaseAnchoredY => _baseAnchoredY;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _coachMovementController = GetComponent<CoachMovementController>();
        _baseAnchoredY = _rectTransform.anchoredPosition.y;
    }

    private void OnEnable()
    {
        if (_bodyButton != null)
            _bodyButton.onClick.AddListener(HandleBodyClicked);

        if (_closeButton != null)
            _closeButton.onClick.AddListener(HandleCloseClicked);
    }

    private void OnDisable()
    {
        if (_bodyButton != null)
            _bodyButton.onClick.RemoveListener(HandleBodyClicked);

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(HandleCloseClicked);
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
        _stackMoveTween?.Kill();
    }

    public void Initialize(UI_ToastSender owner, SO_ToastData data)
    {
        _owner = owner;

        ApplyData(data);
        ResetVisual();
        PlayAnimation(data.displayDuration);
    }

    public void MoveToStackPosition(float targetY, float duration)
    {
        if (_rectTransform == null)
            return;

        _stackMoveTween?.Kill();

        _stackMoveTween = _rectTransform
            .DOAnchorPosY(targetY, duration)
            .SetEase(Ease.OutCubic);
    }

    private void ApplyData(SO_ToastData data)
    {
        if (data == null)
            return;

        if (_profileImage != null)
            _profileImage.sprite = data.profileIcon;

        if (_workspaceNameText != null)
            _workspaceNameText.text = data.workspaceName;

        if (_senderNameText != null)
            _senderNameText.text = data.senderName;

        if (_messageContextText != null)
            _messageContextText.text = data.messageContext;

        if (_toastOutline != null)
            _toastOutline.effectColor = data.outlineColor;
    }

    private void ResetVisual()
    {
        _sequence?.Kill();
        _stackMoveTween?.Kill();

        _canvasGroup.alpha = 1f;
        _rectTransform.anchoredPosition = new Vector2(_hiddenX, _baseAnchoredY);
    }

    private void PlayAnimation(float duration)
    {
        _sequence?.Kill();

        _sequence = DOTween.Sequence();
        _sequence.Append(_rectTransform.DOAnchorPosX(_visibleX, _enterDuration).SetEase(Ease.OutBack));
        _sequence.AppendInterval(duration);
        _sequence.Append(_rectTransform.DOAnchorPosX(_hiddenX, _exitDuration).SetEase(Ease.InBack));
        _sequence.OnComplete(HandleAnimationComplete);
    }

    private void HandleAnimationComplete()
    {
        if (_isClosing)
            return;

        _isClosing = true;

        if (_owner != null)
            _owner.NotifyToastExpired(this);

        Destroy(gameObject);
    }

    private void HandleBodyClicked()
    {
        if (_isSwiped || _isDragging)
            return;

        UIManager_New.Instance.OpenCctvPanel();

        if (_focusCoachRoomOnOpen)
            FocusCoachCurrentRoom();
    }

    private void HandleCloseClicked()
    {
        ForceClose();
    }

    private void FocusCoachCurrentRoom()
    {
        if (CameraManager.Instance == null)
            return;

        if (_coachMovementController == null)
            return;

        RoomID coachRoom = _coachMovementController.CurrentRoomId;
        if (coachRoom == RoomID.None)
            return;

        CameraManager.Instance.SelectCameraByRoomId(coachRoom);
    }

    public void ForceClose()
    {
        if (_isClosing)
            return;

        _isClosing = true;

        _sequence?.Kill();
        _stackMoveTween?.Kill();

        Sequence closeSequence = DOTween.Sequence();
        closeSequence.Append(_rectTransform.DOAnchorPosX(_hiddenX, _exitDuration).SetEase(Ease.InBack));
        closeSequence.OnComplete(() =>
        {
            if (_owner != null)
                _owner.NotifyToastExpired(this);

            Destroy(gameObject);
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isClosing)
            return;

        _isDragging = true;
        _isSwiped = false;
        _dragStartPointerPosition = eventData.position;
        _dragStartAnchoredX = _rectTransform.anchoredPosition.x;

        _sequence?.Pause();
        _stackMoveTween?.Kill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isClosing || _isDragging == false)
            return;

        float deltaX = eventData.position.x - _dragStartPointerPosition.x;
        deltaX = Mathf.Max(0f, deltaX);

        float targetX = _dragStartAnchoredX + deltaX;

        _rectTransform.anchoredPosition = new Vector2(targetX, _rectTransform.anchoredPosition.y);

        if (Mathf.Abs(deltaX) > 10f)
            _isSwiped = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isClosing || _isDragging == false)
            return;

        _isDragging = false;

        float currentX = _rectTransform.anchoredPosition.x;
        float movedDistance = Mathf.Abs(currentX - _dragStartAnchoredX);

        if (movedDistance >= _swipeCloseThreshold)
        {
            _isClosing = true;
            _sequence?.Kill();
            _stackMoveTween?.Kill();

            if (_owner != null)
                _owner.NotifyToastExpired(this);

            Destroy(gameObject);
            return;
        }
    }
}