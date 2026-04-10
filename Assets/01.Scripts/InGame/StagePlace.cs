using UnityEngine;

public class StagePlace : MiniGame
{
    [Header("Place Targets")]
    public PlaceTarget[] targets;


    private int _requireMatchCount;
    private int _currentMatchCount = 0;

    protected override void OnStart()
    {
        _currentMatchCount = 0;
    }
}
