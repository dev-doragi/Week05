using UnityEngine;
using System.Collections.Generic;

public class CoachDebugger : MonoBehaviour
{
    [SerializeField] private CoachMovementController _coach;
    [SerializeField] private GimmickManager _gimmickManager;

    private void OnEnable()
    {
        
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += (from, to) => Debug.Log($"[준비] 코치가 {from}에서 {to}(으)로 갈 준비 중...");
            _coach.OnCoachMoved += (from, to) => Debug.Log($"[이동] 코치가 {from}에서 {to}(으)로 이동 완료. (현재 어그로: {_coach.CurrentAggro})");
            _coach.OnCoachReachedOffice += () => Debug.Log("GameOver");
        }

        if (_gimmickManager != null)
        {
            _gimmickManager.OnLureActivated += LogLureInfluence;
        }
    }

    private void OnDisable()
    {
        if (_gimmickManager != null)
        {
            _gimmickManager.OnLureActivated -= LogLureInfluence;
        }
    }

    private void LogLureInfluence(RoomID lureRoom)
    {
        if (_coach == null || _coach.MapGraph == null) return;

        RoomID coachRoom = _coach.CurrentRoomId;
        IReadOnlyList<RoomID> neighbors = _coach.MapGraph.GetNeighbors(coachRoom);

        bool isEffective = false;
        string neighborList = "";

        foreach (var neighbor in neighbors)
        {
            neighborList += neighbor.ToString() + ", ";
            if (neighbor == lureRoom) isEffective = true;
        }

        if (coachRoom == lureRoom) isEffective = true;

        string status = isEffective ? "<color=green>영향 받음</color>" : "<color=red>영향 없음(너무 멂)</color>";

        Debug.Log($"[사운드 체크] 발생지: {lureRoom} | 코치 위치: {coachRoom} | 주변 경로: [{neighborList.TrimEnd(',', ' ')}]");
        Debug.Log($"[사운드 결과] 코치가 소리를 들을 수 있는가? : {status}");
    }
}