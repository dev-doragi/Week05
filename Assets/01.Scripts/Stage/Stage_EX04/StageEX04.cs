using UnityEngine;

public class StageEX4 : Stage
{
    private UI_InGameEditorRuntimeState _runtimeState;
    [SerializeField]
    private UI_RuntimeReferenceBlocker[] _platformBlockers;

    private void OnEnable()
    {
        CollisionReporter.OnEnter2D += HandleCollision;
        BindRuntimeState();
        RefreshAllPlatforms();
    }

    private void OnDisable()
    {
        CollisionReporter.OnEnter2D -= HandleCollision;
        UnbindRuntimeState();
    }

    private void Start()
    {
        OnStart();
    }
    protected override void OnStart()
    {
        BindRuntimeState();
        RefreshAllPlatforms();

        _runtimeState?.RestoreAllInvalidReferences();

        if (_runtimeState == null)
            return;

        //_runtimeState.TryBreakReference("Platform1", InspectorComponent.BallPop, "reference03");
        //RefreshPlatform("Platform1");
        //_runtimeState.TryBreakReference("Platform7", InspectorComponent.BallPop, "reference03");
        //RefreshPlatform("Platform7");
    }

    private void BindRuntimeState()
    {
        var nextState = InGameEditorController.EditorRuntimeState;
        if (ReferenceEquals(_runtimeState, nextState))
            return;

        UnbindRuntimeState();
        _runtimeState = nextState;

        if (_runtimeState != null)
            _runtimeState.OwnerBlockStateChanged += HandleOwnerBlockStateChanged;
    }

    private void UnbindRuntimeState()
    {
        if (_runtimeState != null)
            _runtimeState.OwnerBlockStateChanged -= HandleOwnerBlockStateChanged;

        _runtimeState = null;
    }

    private void HandleOwnerBlockStateChanged(string ownerId, bool isBlocked)
    {
        SetPlatformMoveState(ownerId, !isBlocked);
    }

    private void RefreshAllPlatforms()
    {
        if (_platformBlockers == null)
            return;

        foreach (var blocker in _platformBlockers)
        {
            if (blocker == null)
                continue;

            blocker.RefreshBlockedState();
            SetPlatformMoveState(blocker.OwnerId, !blocker.IsBlocked);
        }
    }

    private void RefreshPlatform(string ownerId)
    {
        if (_platformBlockers == null || string.IsNullOrEmpty(ownerId))
            return;

        foreach (var blocker in _platformBlockers)
        {
            if (blocker == null || blocker.OwnerId != ownerId)
                continue;

            blocker.RefreshBlockedState();
            SetPlatformMoveState(ownerId, !blocker.IsBlocked);
            return;
        }
    }

    private void SetPlatformMoveState(string ownerId, bool canMove)
    {
        if (_platformBlockers == null || string.IsNullOrEmpty(ownerId))
            return;

        foreach (var blocker in _platformBlockers)
        {
            if (blocker == null || blocker.OwnerId != ownerId)
                continue;

            MovingPlatform platform = blocker.GetComponent<MovingPlatform>();
            if (platform != null)
                platform.canMove = canMove;

            return;
        }
    }

    private void HandleCollision(Collider2D collision)
    {
        MissionClearer mission = collision.GetComponent<MissionClearer>();
        if (mission == null) return;

        mission.ClearMission();
    }
}
