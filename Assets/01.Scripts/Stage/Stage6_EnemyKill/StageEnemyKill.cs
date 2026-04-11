using UnityEngine;

public class StageEnemyKill : Stage
{
    private MissionClearer _mission_Reference;


    protected override void Awake()
    {
        base.Awake();

        _mission_Reference = GetComponent<MissionClearer>();
    }

    protected override void OnStart()
    {
        Invoke(nameof(StageClear), 1.5f);
    }

    private void StageClear()
    {
        _mission_Reference.ClearMission();
    }
}
