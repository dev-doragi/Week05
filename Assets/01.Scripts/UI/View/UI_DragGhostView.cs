using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DragGhostView : MonoBehaviour
{
    public static UI_DragGhostView Instance { get; private set; }

    [SerializeField] private Canvas _canvas;
    private RectTransform _canvasRect;

    [SerializeField] private RectTransform _root;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;

    private void Awake()
    {
        Instance = this;

        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        if (_canvas != null)
            _canvasRect = _canvas.transform as RectTransform;

        if (_canvasGroup != null)
            _canvasGroup.blocksRaycasts = false;

        Hide();
    }



    public void Show(Sprite icon, string displayName)
    {
        gameObject.SetActive(true);

        if (_iconImage != null)
            _iconImage.sprite = icon;

        if (_nameText != null)
            _nameText.text = displayName;
    }

    public void Move(Vector2 screenPosition)
    {
        if (_root == null || _canvasRect == null)
            return;

        Camera eventCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            _root.anchoredPosition = localPoint;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
