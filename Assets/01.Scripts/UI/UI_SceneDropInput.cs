using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UI_SceneDropInput : MonoBehaviour
{
    public static UI_SceneDropInput Instance { get; private set; }

    [SerializeField] private Camera _worldCamera;
    [SerializeField] private InGameEditorController _controller;
    [SerializeField] private float _placementZ = 0f;

    private void Awake()
    {
        Instance = this;
        _worldCamera ??= Camera.main;
        _controller ??= FindFirstObjectByType<InGameEditorController>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool TryHandleDrop(Vector2 screenPosition)
    {
        if (UI_DragContext.IsDragging == false)
            return false;

        if (UI_DragContext.DragType != UIProjectDragType.ScenePlaceablePrefab)
            return false;

        if (UI_DragContext.DraggedPlaceableRecipe == null)
            return false;

        if (IsPointerOverUI())
            return false;

        if (_worldCamera == null || _controller == null)
            return false;

        Vector3 worldPosition = ConvertScreenToWorld(screenPosition);
        _controller.PlaceDraggedPrefab(UI_DragContext.DraggedPlaceableRecipe, worldPosition);
        return true;
    }

    private Vector3 ConvertScreenToWorld(Vector2 screenPosition)
    {
        float distance = Mathf.Abs(_placementZ - _worldCamera.transform.position.z);
        if (distance < 0.01f)
            distance = 0.01f;

        Vector3 worldPosition = _worldCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, distance));
        worldPosition.z = _placementZ;
        return worldPosition;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Mouse.current == null) return false;

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = currentMousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

  
        foreach (var result in results)
        {
            if (result.gameObject.layer == 5)
                return true;
        }
        return false;
    }
}
