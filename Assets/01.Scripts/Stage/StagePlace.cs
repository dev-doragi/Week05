using UnityEngine;

public class StagePlace : MiniGame
{
    [Header("Place Targets")]
    public PlaceTarget[] targets;


    private int _requireMatchCount;
    private int _currentMatchCount = 0;

    private void OnEnable()
    {
        CollisionReporter.OnEnter2D += HandleCollision;
    }


    private void OnDisable()
    {
        CollisionReporter.OnEnter2D -= HandleCollision;
    }

    protected override void OnStart()
    {
        _currentMatchCount = 0;
        _requireMatchCount = targets.Length;
    }


    private void HandleCollision(Collider2D collision)
    {
        if (collision.GetComponent<ClearTrigger>())
        {
            Clear();
        }
    }
}
