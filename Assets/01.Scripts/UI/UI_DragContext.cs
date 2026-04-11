using UnityEngine;

public static class UI_DragContext
{
    public static bool IsDragging { get; private set; }
    public static UIProjectDragType DragType { get; private set; }
    public static string DraggedComponentId { get; private set; }
    public static string DraggedDisplayName { get; private set; }
    public static Sprite DraggedIcon { get; private set; }
    public static SO_UIPlaceablePrefabRecipe DraggedPlaceableRecipe { get; private set; }

    public static void BeginReferenceDrag(string componentId, string displayName, Sprite icon)
    {
        IsDragging = true;
        DragType = UIProjectDragType.ReferenceTarget;
        DraggedComponentId = componentId;
        DraggedDisplayName = displayName;
        DraggedIcon = icon;
        DraggedPlaceableRecipe = null;
    }

    public static void BeginPlaceableDrag(SO_UIPlaceablePrefabRecipe recipe, string displayName, Sprite icon)
    {
        IsDragging = true;
        DragType = UIProjectDragType.ScenePlaceablePrefab;
        DraggedComponentId = recipe != null && recipe.ComponentData != null
            ? recipe.ComponentData.ComponentId
            : null;
        DraggedDisplayName = displayName;
        DraggedIcon = icon;
        DraggedPlaceableRecipe = recipe;
    }

    public static void End()
    {
        IsDragging = false;
        DragType = UIProjectDragType.None;
        DraggedComponentId = null;
        DraggedDisplayName = null;
        DraggedIcon = null;
        DraggedPlaceableRecipe = null;
    }
}
