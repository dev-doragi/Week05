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

    public void IncreaseAggroOnMove()
    {
        _officeAggro += _aggroIncreasePerMove;
    }

    public RoomID GetNextRoom(RoomID currentRoomId)
    {
        IReadOnlyList<RoomID> neighbors = _mapGraph.GetNeighbors(currentRoomId);

        if (neighbors.Count == 0)
            return currentRoomId;

        RoomID targetRoom = _gimmickManager.ActiveTempTarget != RoomID.None
            ? _gimmickManager.ActiveTempTarget
            : RoomID.Office;

        float totalWeight = 0f;
        List<KeyValuePair<RoomID, float>> roomWeights = new List<KeyValuePair<RoomID, float>>();

        foreach (RoomID nextRoom in neighbors)
        {
            float score = CalculateRoomScore(currentRoomId, nextRoom, targetRoom);

            if (score <= 0f)
                continue;

            roomWeights.Add(new KeyValuePair<RoomID, float>(nextRoom, score));
            totalWeight += score;
        }

        if (roomWeights.Count == 0)
            return currentRoomId;

        float randomRoll = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        RoomID selectedRoom = currentRoomId;

        foreach (var roomWeight in roomWeights)
        {
            cumulativeWeight += roomWeight.Value;

            if (randomRoll <= cumulativeWeight)
            {
                selectedRoom = roomWeight.Key;
                break;
            }
        }

        _lastRoomId = currentRoomId;
        return selectedRoom;
    }

    private float CalculateRoomScore(RoomID currentRoom, RoomID nextRoom, RoomID targetRoom)
    {
        if (_gimmickManager.IsPathBlocked(currentRoom, nextRoom))
            return 0f;

        float score = 1f;

        int currentDistance = _mapGraph.GetDistance(currentRoom, targetRoom);
        int nextDistance = _mapGraph.GetDistance(nextRoom, targetRoom);

        if (currentDistance == int.MaxValue || nextDistance == int.MaxValue)
            return score;

        float multiplier = targetRoom != RoomID.Office ? 3.0f : 1.0f;

        if (nextDistance < currentDistance)
        {
            score += (_baseAggroMultiplier * _officeAggro) * multiplier;
        }
        else if (nextDistance > currentDistance)
        {
            score -= (_baseAggroMultiplier * 0.8f) * _officeAggro * multiplier;
        }

        if (nextRoom == _lastRoomId)
        {
            score *= 0.75f;
        }

        return Mathf.Max(0.5f, score);
    }
}