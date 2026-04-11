// AnimTarget.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimTarget : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private AnimPanel _panel;
    [SerializeField] private Vector2 _panelOffset = new Vector2(40f, 40f);

    [Header("Puzzle")]
    [SerializeField] private AnimPairData[] _pairs;

    [Header("Clear Trigger")]
    [SerializeField] private GameObject _clearTrigger;

    [Header("View")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _solvedColor = Color.green;

    public bool IsSolved { get; private set; }

    public void Init(AnimPanel sharedPanel)
    {
        if (sharedPanel != null)
            _panel = sharedPanel;

        IsSolved = false;

        if (_clearTrigger != null)
            _clearTrigger.SetActive(false);

        UpdateView();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsSolved) return;
        if (_canvas == null) return;
        if (_panel == null) return;
        if (_pairs == null || _pairs.Length == 0) return;

        RectTransform canvasRect = (RectTransform)_canvas.transform;

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        if (!success) return;

        _panel.Open(this, _pairs, localPoint + _panelOffset);
    }

    public void Solve()
    {
        if (IsSolved) return;

        IsSolved = true;

        if (_clearTrigger != null)
            _clearTrigger.SetActive(true);

        UpdateView();
    }

    private void UpdateView()
    {
        if (_spriteRenderer == null) return;
        _spriteRenderer.color = IsSolved ? _solvedColor : _normalColor;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
#endif
}
