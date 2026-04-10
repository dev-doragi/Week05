using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameObjectSelectable : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private string _componentId;
    [SerializeField] private UIEditorScene _scene = UIEditorScene.None;
    [SerializeField] private InGameEditorController _controller;

    public string ComponentId => _componentId;
    public UIEditorScene Scene => _scene;
    public event Action<string> ComponentIdChanged;

    private void Awake()
    {
        _controller ??= FindFirstObjectByType<InGameEditorController>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_componentId))
            return;

        _controller?.SelectWorldObject(_componentId, _scene);
    }

    public void Bind(string componentId, UIEditorScene scene)
    {
        bool changed = _componentId != componentId;
        _componentId = componentId;
        _scene = scene;

        if (changed)
            ComponentIdChanged?.Invoke(_componentId);
    }
}
