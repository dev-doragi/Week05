using System;
using UnityEngine;

public class UI_HierarchyItemSlot : MonoBehaviour
{
    [SerializeField] private UI_HierarchyComponentView _view;
    [SerializeField] private UI_ComponentSelectable _selectable;
    [SerializeField] private UI_ComponentDraggable _draggable;

    public event Action<string> Clicked;

    private void Awake()
    {
        _view ??= GetComponent<UI_HierarchyComponentView>();
        _selectable ??= GetComponent<UI_ComponentSelectable>();
        _draggable ??= GetComponent<UI_ComponentDraggable>();

        if (_selectable != null)
            _selectable.Clicked += HandleClicked;
    }

    private void OnDestroy()
    {
        if (_selectable != null)
            _selectable.Clicked -= HandleClicked;
    }

    public void Bind(UI_InGameEditorRuntimeData data, bool isSelected, bool hasReferenceError)
    {
        if (data == null)
            return;

        _view.Render(data.DisplayName, data.SourceData.IconSprite);
        _view.SetErrorBlink(hasReferenceError);

        _selectable.Bind(data.Id);
        _selectable.SetSelected(isSelected);

        if (_draggable != null)
            _draggable.Bind(data.Id, data.DisplayName, data.SourceData.IconSprite);
    }

    private void HandleClicked(string componentId)
    {
        Clicked?.Invoke(componentId);
    }
}
