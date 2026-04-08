using UnityEngine;

public class EditorNullRefGame : MiniGame
{
    private UI_RuntimeReferenceKey _targetKey;

    protected override void OnStart()
    {
        var state = InGameEditorController.EditorRuntimeState;
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
        var state = InGameEditorController.EditorRuntimeState;
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
}
