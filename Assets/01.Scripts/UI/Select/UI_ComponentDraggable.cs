using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ComponentDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private string _componentId;
    private string _displayName;
    private Sprite _iconSprite;
    private UIProjectDragType _dragType;
    private SO_UIPlaceablePrefabRecipe _placeableRecipe;

    public void BindReference(string componentId, string displayName, Sprite iconSprite)
    {
        _dragType = UIProjectDragType.ReferenceTarget;
        _componentId = componentId;
        _displayName = displayName;
        _iconSprite = iconSprite;
        _placeableRecipe = null;
    }

    public void BindPlaceable(SO_UIPlaceablePrefabRecipe recipe, string displayName, Sprite iconSprite)
    {
        _dragType = UIProjectDragType.ScenePlaceablePrefab;
        _componentId = recipe != null && recipe.ComponentData != null
            ? recipe.ComponentData.ComponentId
            : null;
        _displayName = displayName;
        _iconSprite = iconSprite;
        _placeableRecipe = recipe;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        switch (_dragType)
        {
            case UIProjectDragType.ReferenceTarget:
                if (string.IsNullOrEmpty(_componentId))
                    return;

                UI_DragContext.BeginReferenceDrag(_componentId, _displayName, _iconSprite);
                break;

            case UIProjectDragType.ScenePlaceablePrefab:
                if (_placeableRecipe == null)
                    return;

                UI_DragContext.BeginPlaceableDrag(_placeableRecipe, _displayName, _iconSprite);
                break;

            default:
                return;
        }

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
        UI_SceneDropInput.Instance?.TryHandleDrop(eventData.position);
        UI_DragGhostView.Instance?.Hide();
        UI_DragContext.End();
    }
}
