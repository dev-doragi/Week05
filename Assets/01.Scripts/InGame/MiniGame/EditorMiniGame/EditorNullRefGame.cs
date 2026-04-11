using UnityEngine;

public class EditorNullRefGame : MiniGame
{
    [SerializeField] private InspectorComponent _targetComponent;

    private UI_RuntimeReferenceKey _targetKey;
    private UI_InGameEditorRuntimeState _runtimeState;

    private void OnEnable()
    {
        _runtimeState = InGameEditorController.EditorRuntimeState;
    }
    protected override void OnStart()
    {
        if (_runtimeState == null || !_runtimeState.TryBreakReferenceByComponent(_targetComponent, out _targetKey))
        {
            Clear();
            return;
        }

        _runtimeState.ReferenceSolved -= HandleSolved;
        _runtimeState.ReferenceSolved += HandleSolved;
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
