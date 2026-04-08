using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ComponentDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private string _componentId;
    private string _displayName;
    private Sprite _iconSprite;

    public void Bind(string componentId, string displayName, Sprite iconSprite)
    {
        _componentId = componentId;
        _displayName = displayName;
        _iconSprite = iconSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_componentId))
            return;

        UI_DragContext.Begin(_componentId, _displayName, _iconSprite);
        UI_DragGhostView.Instance?.Show(_iconSprite, _displayName);
        UI_DragGhostView.Instance?.Move(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (UI_DragContext.IsDragging == false)
            return;

        UI_DragGhostView.Instance?.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UI_DragGhostView.Instance?.Hide();
        UI_DragContext.End();
    }
}
