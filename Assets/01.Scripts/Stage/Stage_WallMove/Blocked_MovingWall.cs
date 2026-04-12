using Unity.VisualScripting;
using UnityEngine;

public class Blocked_MovingWall : MovingPlatform
{
    private void OnEnable() => StageWallMove.OnWallMove += HandlePlatform;
    private void OnDisable() => StageWallMove.OnWallMove -= HandlePlatform;

    private void HandlePlatform(bool active)
    {
        Debug.Log("[Platform] Move " + active);

        canMove = active;
    }
}
