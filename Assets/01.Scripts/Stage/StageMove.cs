using System;
using UnityEngine;

public class StageMove : Stage
{
    public static Action<bool> OnMove;

    [SerializeField] private UI_RuntimeReferenceBlocker _runtimeReferenceBlocker;
    private MissionClearer _mission_Reference;

    protected override void Awake()
    {
        base.Awake();

        _runtimeReferenceBlocker.GetComponentInChildren<UI_RuntimeReferenceBlocker>();
        _mission_Reference = GetComponent<MissionClearer>();
    }

    private void Start()
    {
        // 테스트용
        OnStart();
    }

    protected override void OnStart()
    {
        if (InGameEditorController.EditorRuntimeState != null)
        {
            InGameEditorController.EditorRuntimeState.
                TryBreakReference("BallPop", InspectorComponent.BallPop, "reference01");
        }
    }

    private void OnEnable()
    {
        _runtimeReferenceBlocker.BlockedChanged += HandleCanMove;
        CollisionReporter.OnEnter2D += HandleCollision;
    }


    private void OnDisable()
    {
        _runtimeReferenceBlocker.BlockedChanged -= HandleCanMove;
        CollisionReporter.OnEnter2D -= HandleCollision;
    }

    private void HandleCanMove(bool active)
    {
        Debug.Log("[Stage Move] Component Reference " + !active);

        OnMove?.Invoke(!active);
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
