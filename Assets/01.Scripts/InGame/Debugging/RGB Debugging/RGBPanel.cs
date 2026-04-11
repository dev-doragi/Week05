using UnityEngine;
using UnityEngine.EventSystems;

public class RGBPanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private RGBObject _object;
    [SerializeField] private Vector2 _offset = new(40f, 40f);

    public RectTransform Panel => _panel;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_panel.gameObject.activeSelf) return;
        if (_object.IsCleared) return;

        RectTransform canvasRect = (RectTransform)_canvas.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );

        _panel.anchoredPosition = localPoint + _offset;
        _panel.gameObject.SetActive(true);
    }
}
