// StageAnim.cs
using UnityEngine;

public class StageAnim : Stage
{
    [SerializeField] private AnimPanel _panel;
    [SerializeField] private AnimTarget[] _targets;

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
        if (_panel != null)
            _panel.Close();

        foreach (AnimTarget target in _targets)
        {
            if (target == null) continue;
            target.Init(_panel);
        }
    }

    private void HandleCollision(Collider2D collision)
    {
        MissionClearer mission = collision.GetComponent<MissionClearer>();
        if (mission == null) return;

        mission.ClearMission();
    }
}
