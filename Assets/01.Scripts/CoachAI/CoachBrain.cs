using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CoachBrain
{
    private MapGraph _mapGraph;
    private GimmickManager _gimmickManager;

    [Header("Aggro Settings")]
    [SerializeField] private float _officeAggro = 1.0f;
    [SerializeField] private float _baseAggroMultiplier = 15f;
    [SerializeField] private float _aggroIncreasePerMove = 0.2f;

    private RoomID _lastRoomId = RoomID.None;
    public float CurrentAggro => _officeAggro;

    public void Initialize(MapGraph mapGraph, GimmickManager gimmickManager)
    {
        _mapGraph = mapGraph;
        _gimmickManager = gimmickManager;
        _officeAggro = 1.0f;
        _lastRoomId = RoomID.None;
    }

    // 코치가 이동할 때마다 어그로(목표 지향성) 증가
    public void IncreaseAggroOnMove()
    {
        _officeAggro += _aggroIncreasePerMove;
    }

    public RoomID GetNextRoom(RoomID currentRoomId)
    {
        IReadOnlyList<RoomID> neighbors = _mapGraph.GetNeighbors(currentRoomId);

        if (neighbors.Count == 0) return currentRoomId;

        RoomID targetRoom = _gimmickManager.ActiveTempTarget != RoomID.None
            ? _gimmickManager.ActiveTempTarget
            : RoomID.Office;

        float totalWeight = 0f;
        List<KeyValuePair<RoomID, float>> roomWeights = new List<KeyValuePair<RoomID, float>>();

        foreach (RoomID nextRoom in neighbors)
        {
            float score = CalculateRoomScore(currentRoomId, nextRoom, targetRoom);

            if (score <= 0f) continue;

            roomWeights.Add(new KeyValuePair<RoomID, float>(nextRoom, score));
            totalWeight += score;
        }

        if (roomWeights.Count == 0) return currentRoomId;

        float randomRoll = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        RoomID selectedRoom = currentRoomId;
        foreach (var rw in roomWeights)
        {
            cumulativeWeight += rw.Value;
            if (randomRoll <= cumulativeWeight)
            {
                selectedRoom = rw.Key;
                break;
            }
        }

        _lastRoomId = currentRoomId;
        return selectedRoom;
    }

    private float CalculateRoomScore(RoomID currentRoom, RoomID nextRoom, RoomID targetRoom)
    {
        // 기믹 1: 경로 차단 확인
        if (_gimmickManager.IsPathBlocked(currentRoom, nextRoom))
        {
            return 0f; // 이동 불가
        }

        // 그래프에 정의된 기본 확률 가중치 가져오기
        float score = _mapGraph.GetBaseWeight(currentRoom, nextRoom);

        // 목표 지점과의 거리 비교
        int currentDistance = _mapGraph.GetDistance(currentRoom, targetRoom);
        int nextDistance = _mapGraph.GetDistance(nextRoom, targetRoom);

        // 거리 계산 예외 처리 (도달 불가능한 노드)
        if (currentDistance == MapGraph.UnreachableDistance || nextDistance == MapGraph.UnreachableDistance)
        {
            return score;
        }

        // 어그로(시간)에 따른 방향 가중치 적용
        if (nextDistance < currentDistance)
        {
            // 목표와 가까워지는 방향: 어그로 수치만큼 가중치 추가
            score += _baseAggroMultiplier * _officeAggro;
        }
        else if (nextDistance > currentDistance)
        {
            // 목표와 멀어지는 방향: 어그로 수치에 비례하여 페널티 부여
            // 초반엔 역방향 이동이 잦지만, 후반엔 역방향 점수가 크게 깎임
            score -= (_baseAggroMultiplier * 0.8f) * _officeAggro;
        }

        // 기믹 2: 소리 유인 수치 적용
        float lureValue = _gimmickManager.GetLureValue(nextRoom);
        if (lureValue > 0f)
        {
            // 어그로가 높을수록 유인 효과가 감소하도록 계산
            score += lureValue / Mathf.Max(1f, _officeAggro * 0.5f);
        }

        if (nextRoom == _lastRoomId)
        {
            score *= 0.75f;
        }

        // 점수가 너무 낮아져도 최소한의 이동 가능성은 보장 (역방향 페널티로 인한 음수 방지)
        return Mathf.Max(0.5f, score);
    }
}