using UnityEngine;

public class Blocked_MovingPlatform : MovingPlatform
{
    private void OnEnable() => StagePlatformMove.OnPlatformMove += HandlePlatform;
    private void OnDisable() => StagePlatformMove.OnPlatformMove -= HandlePlatform;

    private void HandlePlatform(bool active)
    {
        Debug.Log("[Platform] Move " + active);

        canMove = active;
    }
}
