using System;
using UnityEngine;

public class StageMove : Stage
{
    public static Action<bool> OnMove;

    [SerializeField] private UI_RuntimeReferenceBlocker _runtimeReferenceBlocker;

    protected override void Awake()
    {
        base.Awake();

        _runtimeReferenceBlocker.GetComponentInChildren<UI_RuntimeReferenceBlocker>();
    }
        

    protected override void OnStart()
    {
        HandleCanMove(false);
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
        Debug.Log("[Stage] Component Reference " + !active);

        OnMove?.Invoke(!active);
    }

    private void HandleCollision(Collider2D collision)
    {
        MissionClearer clear;
        if (clear = collision.GetComponent<MissionClearer>())
        {
            clear.ClearMission();
        }
    }
}
