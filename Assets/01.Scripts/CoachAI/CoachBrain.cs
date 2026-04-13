using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CoachMoveCandidate
{
    public RoomID RoomId;
    public float Score;

    public CoachMoveCandidate(RoomID roomId, float score)
    {
        RoomId = roomId;
        Score = score;
    }
}

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
    private readonly List<CoachMoveCandidate> _cachedCandidates = new();

    public float CurrentAggro => _officeAggro;
    public IReadOnlyList<CoachMoveCandidate> CachedCandidates => _cachedCandidates;

    public void Initialize(MapGraph mapGraph, GimmickManager gimmickManager)
    {
        _mapGraph = mapGraph;
        _gimmickManager = gimmickManager;
        _officeAggro = 1.0f;
        _lastRoomId = RoomID.None;
        _cachedCandidates.Clear();
    }

    public void IncreaseAggroOnMove()
    {
        _officeAggro += _aggroIncreasePerMove;
    }

    public IReadOnlyList<CoachMoveCandidate> GetMoveCandidates(RoomID currentRoomId)
    {
        _cachedCandidates.Clear();

        if (_mapGraph == null)
            return _cachedCandidates;

        IReadOnlyList<RoomID> neighbors = _mapGraph.GetNeighbors(currentRoomId);
        if (neighbors.Count == 0)
            return _cachedCandidates;

        RoomID targetRoom = RoomID.Office;

        if (_gimmickManager != null && _gimmickManager.ActiveTempTarget != RoomID.None)
        {
            if (currentRoomId == _gimmickManager.ActiveTempTarget)
                return _cachedCandidates;

            targetRoom = _gimmickManager.ActiveTempTarget;
        }

        for (int i = 0; i < neighbors.Count; i++)
        {
            RoomID nextRoomId = neighbors[i];
            float score = CalculateRoomScore(currentRoomId, nextRoomId, targetRoom);

            if (score <= 0f)
                continue;

            _cachedCandidates.Add(new CoachMoveCandidate(nextRoomId, score));
        }

        return _cachedCandidates;
    }

    public void CommitSelectedMove(RoomID currentRoomId)
    {
        _lastRoomId = currentRoomId;
    }

    private float CalculateRoomScore(RoomID currentRoomId, RoomID nextRoomId, RoomID targetRoomId)
    {
        float score = 1f;

        int currentDistance = _mapGraph.GetDistance(currentRoomId, targetRoomId);
        int nextDistance = _mapGraph.GetDistance(nextRoomId, targetRoomId);

        if (currentDistance == int.MaxValue || nextDistance == int.MaxValue)
            return score;

        float multiplier = targetRoomId != RoomID.Office ? 3.0f : 1.0f;

        if (nextDistance < currentDistance)
        {
            score += (_baseAggroMultiplier * _officeAggro) * multiplier;
        }
        else if (nextDistance > currentDistance)
        {
            score -= (_baseAggroMultiplier * 0.8f) * _officeAggro * multiplier;
        }

        if (nextRoomId == _lastRoomId)
            score *= 0.75f;

        return Mathf.Max(0.5f, score);
    }
}