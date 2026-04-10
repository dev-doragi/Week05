using System;
using UnityEngine;

public class StageMove : MiniGame
{
    public static Action<bool> OnMove;

    [SerializeField] private UI_RuntimeReferenceBlocker _runtimeReferenceBlocker;

    private void Awake() => _runtimeReferenceBlocker.GetComponentInChildren<UI_RuntimeReferenceBlocker>();

    protected override void OnStart()
    {
        HandleCanMove(false);
    }

    private void OnEnable() => _runtimeReferenceBlocker.BlockedChanged += HandleCanMove;

    private void OnDisable() => _runtimeReferenceBlocker.BlockedChanged -= HandleCanMove;

    private void HandleCanMove(bool active)
    {
        Debug.Log("[Stage] Component Reference " + active);

        OnMove?.Invoke(!active);
    }
}
