using UnityEngine;

public class StagePlace : Stage
{
    [Header("Place Targets")]
    public PlaceTarget[] targets;

    private MissionClearer _mission_Reference;

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

    private void Start()
    {
        _mission_Reference = GetComponent<MissionClearer>();

        // 테스트용
        //OnStart();
    }

    protected override void OnStart()
    {
        _currentMatchCount = 0;
        _requireMatchCount = targets.Length;

        foreach (var t in targets)
        {
            t.Init();
            t.OnMatch += HandleMatch;
        }

        Debug.Log("[Stage Place] Start. Required Match: " + _requireMatchCount);
    }

    private void HandleMatch()
    {
        _currentMatchCount++;
        Debug.Log($"[Stage Place] Match Success! Progress: {_currentMatchCount} / {_requireMatchCount}");

        if (_currentMatchCount >= _requireMatchCount)
        {
            Debug.Log("[Stage Place] All Targets Matched!");
            _mission_Reference.ClearMission();
        }
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
