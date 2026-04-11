using System;
using UnityEngine;

public class StagePlatformMove : Stage
{
    public static Action<bool> OnPlatformMove;

    [SerializeField] private UI_RuntimeReferenceBlocker _runtimeReferenceBlocker;
    private MissionClearer _mission_Reference;


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
                TryBreakReference("Bomb", InspectorComponent.Bomb, "reference01");
        }
    }

    private void HandlePlatformMove(bool active)
    {
        Debug.Log("[Stage Move] Component Reference " + !active);

        OnPlatformMove?.Invoke(!active);
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
