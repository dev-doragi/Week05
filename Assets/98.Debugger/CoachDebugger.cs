using UnityEngine;

public class CoachDebugger : MonoBehaviour
{
    [SerializeField] private CoachMovementController _coach;

    private GimmickManager _gimmickManager;

    private void Start()
    {
        // 1. 모든 오브젝트의 Awake가 끝난 Start 시점에 싱글톤 참조
        _gimmickManager = GimmickManager.Instance;

        // 2. 참조가 보장된 상태에서 이벤트 구독
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += HandleCoachPreparing;
            _coach.OnCoachMoved += HandleCoachMoved;
            _coach.OnCoachReachedOffice += HandleCoachReachedOffice;
        }

        if (_gimmickManager != null)
            _gimmickManager.OnStayDelayActivated += LogLureInfluence;
    }

    private void OnDestroy()
    {
        // Start에서 구독한 이벤트는 오브젝트 파괴 시점에 해제
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