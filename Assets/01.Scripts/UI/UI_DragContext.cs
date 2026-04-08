using UnityEngine;

public static class UI_DragContext
{
    public static bool IsDragging { get; private set; }
    public static string DraggedComponentId { get; private set; }
    public static string DraggedDisplayName { get; private set; }
    public static Sprite DraggedIcon { get; private set; }

    public static void Begin(string componentId, string displayName, Sprite icon)
    {
        IsDragging = true;
        DraggedComponentId = componentId;
        DraggedDisplayName = displayName;
        DraggedIcon = icon;
    }

    public static void End()
    {
        IsDragging = false;
        DraggedComponentId = null;
        DraggedDisplayName = null;
        DraggedIcon = null;
    }
}
