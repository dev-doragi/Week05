using UnityEngine;

public class StageAnim : Stage
{
    [SerializeField] private AnimObject[] _targets;

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
        foreach (AnimObject target in _targets)
        {
            if (target == null) continue;
            target.Init();
        }
    }

    private void HandleCollision(Collider2D collision)
    {
        MissionClearer mission = collision.GetComponent<MissionClearer>();
        if (mission == null) return;

        mission.ClearMission();
    }
}
