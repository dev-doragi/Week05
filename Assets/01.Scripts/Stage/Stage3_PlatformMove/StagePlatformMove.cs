using System;
using UnityEngine;

public class StagePlatformMove : Stage
{
    public static Action<bool> OnPlatformMove;

    [SerializeField] private UI_RuntimeReferenceBlocker _runtimeReferenceBlocker;
    private MissionClearer _mission_Reference;


    protected override void Awake()
    {
        base.Awake();

        if (_runtimeReferenceBlocker == null) 
            _runtimeReferenceBlocker.GetComponentInChildren<UI_RuntimeReferenceBlocker>();
        _mission_Reference = GetComponent<MissionClearer>();
    }

    private void OnEnable()
    {
        _runtimeReferenceBlocker.BlockedChanged += HandlePlatformMove;
        CollisionReporter.OnEnter2D += HandleCollision;
    }


    private void OnDisable()
    {
        _runtimeReferenceBlocker.BlockedChanged -= HandlePlatformMove;
        CollisionReporter.OnEnter2D -= HandleCollision;
    }

    protected override void OnStart()
    {
        if (InGameEditorController.EditorRuntimeState != null)
        {
            InGameEditorController.EditorRuntimeState.
                TryBreakReference("MovingPlatform", InspectorComponent.BallPop, "reference03");
        }
    }

    private void HandlePlatformMove(bool blocked)
    {
        // Blocked는 컴포넌트 기준이기 때문에, (Blocked == True: 컴포넌트 빠짐)1
        // 플레이어 입장에서는 Blocked가 false일 때 움직일 수 있어야 함
        bool active = !blocked;
        Debug.Log("[Stage Move] Component Reference " + active);

        OnPlatformMove?.Invoke(active);
        if (active) _mission_Reference.ClearMission();
    }

    private void HandleCollision(Collider2D collision)
    {
        MissionClearer _mission_Clear;
        if (_mission_Clear = collision.GetComponent<MissionClearer>())
        {
            _mission_Clear.ClearMission();
        }
    }
}
