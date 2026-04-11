using UnityEngine;

public class StageEnemyKill : Stage
{
    [Header("Setting")]
    [SerializeField] private float _clearDelay;

    private MissionClearer _mission_Reference;


    protected override void Awake()
    {
        base.Awake();

        _mission_Reference = GetComponent<MissionClearer>();
    }

    protected override void OnStart()
    {
        Invoke(nameof(StageClear), _clearDelay);
    }

    private void StageClear()
    {
        _mission_Reference.ClearMission();
    }
}
