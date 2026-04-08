using UnityEngine;

public class EditorNullRefGame : MiniGame
{
    private UI_RuntimeReferenceKey _targetKey;
    private InGameEditorController _controller;

    protected override void OnStart()
    {
        var state = GetRuntimeState();
        if (state == null || !state.TryBreakRandomReference(out _targetKey))
        {
            Clear();
            return;
        }

        state.ReferenceSolved -= HandleSolved;
        state.ReferenceSolved += HandleSolved;
    }

    private void OnDisable()
    {
        var state = GetRuntimeState();
        if (state != null)
            state.ReferenceSolved -= HandleSolved;
    }

    private void HandleSolved(UI_RuntimeReferenceKey key)
    {
        if (key.OwnerId == _targetKey.OwnerId &&
            key.InspectorComponent == _targetKey.InspectorComponent &&
            key.SlotId == _targetKey.SlotId)
        {
            Clear();
        }
    }

    private UI_InGameEditorRuntimeState GetRuntimeState()
    {
        if (_controller == null)
            _controller = FindFirstObjectByType<InGameEditorController>();

        return _controller != null ? _controller.EditorRuntimeState : null;
    }
}
