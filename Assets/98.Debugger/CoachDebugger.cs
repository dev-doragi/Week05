using UnityEngine;

public class CoachDebugger : MonoBehaviour
{
    [SerializeField] private CoachMovementController _coach;

    private GimmickManager _gimmickManager;

    private void OnEnable()
    {
        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += HandleCoachPreparing;
            _coach.OnCoachMoved += HandleCoachMoved;
            _coach.OnCoachPathBlocked += HandleCoachPathBlocked;
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
            _coach.OnCoachPathBlocked -= HandleCoachPathBlocked;
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
        if (_coach == null)
            return;

        Debug.Log($"[이동] 코치가 {from}에서 {to}(으)로 이동 완료. (현재 어그로: {_coach.CurrentAggro}, 현재 상태: {_coach.CurrentAggroState})");
    }

    private void HandleCoachPathBlocked(RoomID from, RoomID to)
    {
        Debug.Log($"[길막힘] 코치가 {from} -> {to} 경로로 이동하려 했지만 막혀서 실패.");
    }

    private void HandleCoachReachedOffice()
    {
        Debug.Log("GameOver");
        GameManager.Instance.GameOver();
    }

    private void LogLureInfluence(GimmickType gimmickType)
    {
        if (_coach == null || _gimmickManager == null)
            return;

        RoomID coachRoom = _coach.CurrentRoomId;
        float extraStayTime = _gimmickManager.GetExtraStayTime();

        Debug.Log($"[기믹 발동] 타입: {gimmickType} | 코치 현재 방: {coachRoom} | 추가 체류 시간: {extraStayTime}초");
    }
}