using System.Collections.Generic;
using UnityEngine;

public enum CoachAggroState
{
    None,
    Wander,
    Alert,
    Chase
}

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

    [Header("Aggro Value")]
    [Tooltip("현재 Office 집착 수치입니다. 이동할수록 증가하며 상태 전환 기준으로 사용됩니다.")]
    [SerializeField] private float _officeAggro = 0f;

    [Tooltip("어그로가 증가할 수 있는 최대값입니다.")]
    [SerializeField] private float _maxAggro = 1f;

    [Tooltip("코치가 한 번 이동할 때마다 증가하는 어그로 값입니다.")]
    [SerializeField] private float _aggroIncreasePerMove = 0.1f;

    [Header("Aggro State Threshold")]
    [Tooltip("이 값 이상이면 Wander에서 Alert 상태로 전환됩니다.")]
    [SerializeField] private float _alertThreshold = 0.35f;

    [Tooltip("이 값 이상이면 Alert에서 Chase 상태로 전환됩니다.")]
    [SerializeField] private float _chaseThreshold = 0.75f;

    [Header("Score Weights")]
    [Tooltip("Wander 상태에서 목표에 가까워지는 방을 선택할 때 추가되는 점수입니다.")]
    [SerializeField] private float _wanderCloserBonus = 0.15f;

    [Tooltip("Wander 상태에서 목표에서 멀어지는 방을 선택할 때 감소되는 점수입니다.")]
    [SerializeField] private float _wanderFartherPenalty = 0.05f;

    [Tooltip("Alert 상태에서 목표에 가까워지는 방을 선택할 때 추가되는 점수입니다.")]
    [SerializeField] private float _alertCloserBonus = 1.25f;

    [Tooltip("Alert 상태에서 목표에서 멀어지는 방을 선택할 때 감소되는 점수입니다.")]
    [SerializeField] private float _alertFartherPenalty = 0.45f;

    [Tooltip("Chase 상태에서 목표에 가까워지는 방을 선택할 때 추가되는 점수입니다.")]
    [SerializeField] private float _chaseCloserBonus = 3.5f;

    [Tooltip("Chase 상태에서 목표에서 멀어지는 방을 선택할 때 감소되는 점수입니다.")]
    [SerializeField] private float _chaseFartherPenalty = 1.2f;

    [Header("Etc")]
    [Tooltip("직전에 있던 방으로 되돌아가려 할 때 곱해지는 패널티 배수입니다.")]
    [SerializeField] private float _backtrackPenaltyMultiplier = 0.7f;

    [Tooltip("임시 목표가 활성화되어 있을 때 최종 점수에 곱해지는 배수입니다.")]
    [SerializeField] private float _tempTargetMultiplier = 1.5f;

    [Tooltip("점수가 너무 낮아져 후보에서 완전히 사라지지 않도록 보장하는 최소 점수입니다.")]
    [SerializeField] private float _minimumScore = 0.2f;

    private RoomID _lastRoomId = RoomID.None;
    private readonly List<CoachMoveCandidate> _cachedCandidates = new();

    public float CurrentAggro => _officeAggro;
    public CoachAggroState CurrentAggroState => EvaluateAggroState(_officeAggro);
    public IReadOnlyList<CoachMoveCandidate> CachedCandidates => _cachedCandidates;

    public void Initialize(MapGraph mapGraph, GimmickManager gimmickManager)
    {
        _mapGraph = mapGraph;
        _gimmickManager = gimmickManager;
        _officeAggro = 0f;
        _lastRoomId = RoomID.None;
        _cachedCandidates.Clear();
    }

    public void IncreaseAggroOnMove()
    {
        _officeAggro = Mathf.Clamp(_officeAggro + _aggroIncreasePerMove, 0f, _maxAggro);
    }

    public void SetAggro(float aggro)
    {
        _officeAggro = Mathf.Clamp(aggro, 0f, _maxAggro);
    }

    public IReadOnlyList<CoachMoveCandidate> GetMoveCandidates(RoomID currentRoomId)
    {
        _cachedCandidates.Clear();

        if (_mapGraph == null)
            return _cachedCandidates;

        IReadOnlyList<RoomID> neighbors = _mapGraph.GetNeighbors(currentRoomId);
        if (neighbors.Count == 0)
            return _cachedCandidates;

        RoomID targetRoomId = RoomID.Office;
        bool hasTempTarget = false;

        if (_gimmickManager != null && _gimmickManager.ActiveTempTarget != RoomID.None)
        {
            if (currentRoomId == _gimmickManager.ActiveTempTarget)
                return _cachedCandidates;

            targetRoomId = _gimmickManager.ActiveTempTarget;
            hasTempTarget = true;
        }

        CoachAggroState aggroState = CurrentAggroState;

        for (int i = 0; i < neighbors.Count; i++)
        {
            RoomID nextRoomId = neighbors[i];
            float score = CalculateRoomScore(currentRoomId, nextRoomId, targetRoomId, aggroState, hasTempTarget);

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

    private CoachAggroState EvaluateAggroState(float aggro)
    {
        if (aggro >= _chaseThreshold)
            return CoachAggroState.Chase;

        if (aggro >= _alertThreshold)
            return CoachAggroState.Alert;

        return CoachAggroState.Wander;
    }

    private float CalculateRoomScore(
        RoomID currentRoomId,
        RoomID nextRoomId,
        RoomID targetRoomId,
        CoachAggroState aggroState,
        bool hasTempTarget)
    {
        float score = 1f;

        int currentDistance = _mapGraph.GetDistance(currentRoomId, targetRoomId);
        int nextDistance = _mapGraph.GetDistance(nextRoomId, targetRoomId);

        if (currentDistance == int.MaxValue || nextDistance == int.MaxValue)
            return score;

        float closerBonus = 0f;
        float fartherPenalty = 0f;

        switch (aggroState)
        {
            case CoachAggroState.Wander:
                closerBonus = _wanderCloserBonus;
                fartherPenalty = _wanderFartherPenalty;
                break;

            case CoachAggroState.Alert:
                closerBonus = _alertCloserBonus;
                fartherPenalty = _alertFartherPenalty;
                break;

            case CoachAggroState.Chase:
                closerBonus = _chaseCloserBonus;
                fartherPenalty = _chaseFartherPenalty;
                break;
        }

        if (nextDistance < currentDistance)
        {
            score += closerBonus;
        }
        else if (nextDistance > currentDistance)
        {
            score -= fartherPenalty;
        }

        if (nextRoomId == _lastRoomId)
            score *= _backtrackPenaltyMultiplier;

        if (hasTempTarget)
            score *= _tempTargetMultiplier;

        return Mathf.Max(_minimumScore, score);
    }
}