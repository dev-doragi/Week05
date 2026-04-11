// AnimPanel.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private AnimObject _owner;
    [SerializeField] private Vector2 _offset = new Vector2(40f, 40f);

    public bool IsOpen => _panel != null && _panel.gameObject.activeSelf;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_owner == null) return;
        if (_owner.IsCleared) return;
        if (_panel.gameObject.activeSelf) return;

        _owner.OpenPanel(eventData);
    }

    public void Show(PointerEventData eventData)
    {
        if (_canvas == null) return;
        if (_panel == null) return;

        RectTransform canvasRect = (RectTransform)_canvas.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        _panel.anchoredPosition = localPoint + _offset;
        _panel.gameObject.SetActive(true);
    }

    public void Close()
    {
        if (_panel == null) return;
        _panel.gameObject.SetActive(false);
    }
}
