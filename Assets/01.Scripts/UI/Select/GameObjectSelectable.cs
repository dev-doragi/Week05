using UnityEngine;
using UnityEngine.EventSystems;


public class GameObjectSelectable : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private string _componentId;
    [SerializeField] private UIEditorScene _scene = UIEditorScene.None;
    [SerializeField] private InGameEditorController _controller;

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
        _componentId = componentId;
        _scene = scene;
    }
}