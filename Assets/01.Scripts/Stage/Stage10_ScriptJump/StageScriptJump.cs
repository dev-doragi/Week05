using System;
using UnityEngine;

public class StageScriptJump : Stage
{
    public static Action<bool> OnJump;
    [SerializeField] private TypingPanel _typingPanel;
    [SerializeField] private MissionClearer _missionClearerCanJump;


    protected override void OnStart()
    {
        Debug.Log("[Stage ScriptJump] Start.");
    }

    private void OnEnable()
    {
        _typingPanel.OnTypingComplete += HandleCanJump;
        CollisionReporter.OnEnter2D += HandleCollision;
    }

    private void OnDisable()
    {
        _typingPanel.OnTypingComplete -= HandleCanJump;
        CollisionReporter.OnEnter2D -= HandleCollision;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //테스트용
        //OnStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HandleCanJump(bool active)
    {
        Debug.Log("[Stage Script Jump] Jump " + active);
        _missionClearerCanJump.ClearMission();
        OnJump?.Invoke(active);
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
