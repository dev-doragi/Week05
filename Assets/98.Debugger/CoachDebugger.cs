using UnityEngine;
using System.Collections.Generic;

public class CoachDebugger : MonoBehaviour
{
    [SerializeField] private CoachMovementController _coach;
    [SerializeField] private BuildFailEndingController _endingController;

    private GimmickManager _gimmickManager;

    private void Awake()
    {
        _gimmickManager = GimmickManager.Instance;
    }

    private void OnEnable()
    {
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += HandleCoachPreparing;
            _coach.OnCoachMoved += HandleCoachMoved;
            _coach.OnCoachReachedOffice += HandleCoachReachedOffice;
        }

        if (_gimmickManager != null)
            _gimmickManager.OnStayDelayActivated += LogLureInfluence;
    }

    private void OnDisable()
    {
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove -= HandleCoachPreparing;
            _coach.OnCoachMoved -= HandleCoachMoved;
            _coach.OnCoachReachedOffice -= HandleCoachReachedOffice;
        }

        if (_gimmickManager != null)
            _gimmickManager.OnStayDelayActivated -= LogLureInfluence;
    }

    private void HandleCoachPreparing(RoomID from, RoomID to)
    {
        Debug.Log($"[준비] 코치가 {from}에서 {to}(으)로 갈 준비 중...");
    }

    private void HandleCoachMoved(RoomID from, RoomID to)
    {
        Debug.Log($"[이동] 코치가 {from}에서 {to}(으)로 이동 완료. (현재 어그로: {_coach.CurrentAggro})");
    }

    private void HandleCoachReachedOffice()
    {
        Debug.Log("GameOver");
        _endingController?.PlayBuildFailSequence();
    }
    private void LogLureInfluence()
    {
        if (_coach == null || _gimmickManager == null)
            return;

        RoomID coachRoom = _coach.CurrentRoomId;
        float extraStayTime = _gimmickManager.GetExtraStayTime();

        Debug.Log($"[사운드 머무름] 코치 현재 방: {coachRoom} | 추가 체류 시간: {extraStayTime}초");
    }
}

