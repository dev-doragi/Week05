using System;
using UnityEngine;

public class StageMakeTrampoline : Stage
{
    [Header("Place Targets")]
    public PlaceTarget target;

    public static Action<bool> OnJump;
    [SerializeField] private TypingPanel _typingPanel;
    [SerializeField] private MissionClearer _missionClearerScriptTampoline;
    [SerializeField] private MissionClearer _missionClearerPlaceTrampoline;


    protected override void OnStart()
    {
        Debug.Log("[Stage ScriptJump] Start.");
        _typingPanel.OpenPanel();
        target.Init();
        target.OnMatch += HandleMatch;
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
        OnJump?.Invoke(active);
        _missionClearerScriptTampoline.ClearMission();
    }

    private void HandleMatch()
    {
        _missionClearerPlaceTrampoline.ClearMission();
        
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
