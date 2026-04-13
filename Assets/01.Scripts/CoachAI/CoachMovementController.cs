using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoachMovementController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _initialSpawnDelay = 5f;
    [SerializeField] private float _timeToRoomChange = 8f;
    [SerializeField] private float _timerVariability = 2f;
    [SerializeField] private float _transitionDuration = 2f;

    [Header("Dependencies")]
    [SerializeField] private CoachBrain _coachBrain;

    [Header("Debug")]
    [SerializeField] private RoomID _currentRoomId = RoomID.None;
    [SerializeField] private bool _isActive;

    private bool _isTransitioning;
    private bool _isSpawnDelayed = true;
    private float _moveTimer;
    private RoomID _nextRoomId = RoomID.None;
    private Coroutine _moveRoutine;
    private Coroutine _initialDelayRoutine;
    private MapGraph _mapGraph;
    private GimmickManager _gimmickManager;

    public CoachBrain CoachBrain => _coachBrain;
    public RoomID CurrentRoomId => _currentRoomId;
    public RoomID NextRoomId => _nextRoomId;
    public MapGraph MapGraph => _mapGraph;
    public bool IsTransitioning => _isTransitioning;
    public float TransitionDuration => _transitionDuration;
    public float CurrentAggro => _coachBrain != null ? _coachBrain.CurrentAggro : 0f;
    public CoachAggroState CurrentAggroState => _coachBrain != null ? _coachBrain.CurrentAggroState : CoachAggroState.None;

    public event Action<RoomID, RoomID> OnCoachMoved;
    public event Action<RoomID, RoomID> OnCoachPreparingToMove;
    public event Action<RoomID, RoomID> OnCoachPathBlocked;
    public event Action OnCoachReachedOffice;

    private void Awake()
    {
        _mapGraph = new MapGraph();

        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        _currentRoomId = RoomID.None;
        _isSpawnDelayed = true;
        _isActive = false;

        if (_coachBrain != null)
            _coachBrain.Initialize(_mapGraph, _gimmickManager);
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += StartMovement;

        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        if (_gimmickManager != null)
            _gimmickManager.OnStayDelayActivated += HandleStayDelayActivated;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= StartMovement;

        if (_gimmickManager != null)
            _gimmickManager.OnStayDelayActivated -= HandleStayDelayActivated;
    }

    private void Update()
    {
        if (_isActive == false || _isTransitioning || _isSpawnDelayed || _currentRoomId == RoomID.Office)
            return;

        _moveTimer -= Time.deltaTime;

        if (_moveTimer <= 0f)
            TryMoveNext();
    }

    public void StartMovement()
    {
        if (_isActive)
            return;

        _isActive = true;

        if (_initialDelayRoutine != null)
            StopCoroutine(_initialDelayRoutine);

        _initialDelayRoutine = StartCoroutine(CoInitialDelay());
    }

    public void StopMovement()
    {
        _isActive = false;

        if (_initialDelayRoutine != null)
        {
            StopCoroutine(_initialDelayRoutine);
            _initialDelayRoutine = null;
        }

        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        _isTransitioning = false;
        _nextRoomId = RoomID.None;
    }

    private IEnumerator CoInitialDelay()
    {
        yield return new WaitForSeconds(_initialSpawnDelay);

        _currentRoomId = GetRandomSpawnRoom();
        _isSpawnDelayed = false;
        ResetMoveTimer();

        OnCoachMoved?.Invoke(RoomID.None, _currentRoomId);
        _initialDelayRoutine = null;
    }

    public void ForceMoveTo(RoomID targetRoomId)
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        RoomID previousRoomId = _currentRoomId;

        _currentRoomId = targetRoomId;
        _nextRoomId = RoomID.None;
        _isTransitioning = false;
        _isSpawnDelayed = false;

        ResetMoveTimer();
        OnCoachMoved?.Invoke(previousRoomId, _currentRoomId);

        if (_currentRoomId == RoomID.Office)
            OnCoachReachedOffice?.Invoke();
    }

    private void TryMoveNext()
    {
        if (_coachBrain == null)
        {
            ResetMoveTimer();
            return;
        }

        IReadOnlyList<CoachMoveCandidate> candidates = _coachBrain.GetMoveCandidates(_currentRoomId);
        if (candidates == null || candidates.Count == 0)
        {
            ResetMoveTimer();
            return;
        }

        float highestScore = float.MinValue;
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].Score > highestScore)
                highestScore = candidates[i].Score;
        }

        const float scoreTolerance = 0.001f;

        bool hasBlockedTopCandidate = false;
        RoomID blockedTopRoomId = RoomID.None;
        List<CoachMoveCandidate> selectableCandidates = new List<CoachMoveCandidate>(candidates.Count);

        for (int i = 0; i < candidates.Count; i++)
        {
            CoachMoveCandidate candidate = candidates[i];
            bool isBlocked = _gimmickManager != null && _gimmickManager.IsPathBlocked(_currentRoomId, candidate.RoomId);
            bool isTopCandidate = Mathf.Abs(candidate.Score - highestScore) <= scoreTolerance;

            if (isBlocked && isTopCandidate && hasBlockedTopCandidate == false)
            {
                hasBlockedTopCandidate = true;
                blockedTopRoomId = candidate.RoomId;
            }

            if (isBlocked == false)
                selectableCandidates.Add(candidate);
        }

        if (hasBlockedTopCandidate)
            OnCoachPathBlocked?.Invoke(_currentRoomId, blockedTopRoomId);

        if (selectableCandidates.Count == 0)
        {
            ResetMoveTimer();
            return;
        }

        RoomID nextRoomId = SelectWeightedRoom(selectableCandidates, _currentRoomId);

        if (nextRoomId == _currentRoomId || nextRoomId == RoomID.None)
        {
            ResetMoveTimer();
            return;
        }

        _coachBrain.CommitSelectedMove(_currentRoomId);

        _nextRoomId = nextRoomId;
        _isTransitioning = true;

        OnCoachPreparingToMove?.Invoke(_currentRoomId, _nextRoomId);

        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(CoMoveAfterDelay());
    }

    private RoomID SelectWeightedRoom(List<CoachMoveCandidate> candidates, RoomID fallbackRoomId)
    {
        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
            totalWeight += candidates[i].Score;

        if (totalWeight <= 0f)
            return fallbackRoomId;

        float randomRoll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulativeWeight += candidates[i].Score;

            if (randomRoll <= cumulativeWeight)
                return candidates[i].RoomId;
        }

        return candidates[candidates.Count - 1].RoomId;
    }

    private IEnumerator CoMoveAfterDelay()
    {
        yield return new WaitForSeconds(_transitionDuration);

        RoomID previousRoomId = _currentRoomId;

        _currentRoomId = _nextRoomId;
        _nextRoomId = RoomID.None;
        _isTransitioning = false;
        _moveRoutine = null;

        _coachBrain.IncreaseAggroOnMove();
        ResetMoveTimer();
        OnCoachMoved?.Invoke(previousRoomId, _currentRoomId);

        if (_currentRoomId == RoomID.Office)
            OnCoachReachedOffice?.Invoke();
    }

    private void HandleStayDelayActivated(GimmickType gimmickType)
    {
        if (_isActive == false || _isTransitioning || _isSpawnDelayed || _currentRoomId == RoomID.Office)
            return;

        if (_gimmickManager == null)
            return;

        _moveTimer += _gimmickManager.GetExtraStayTime();
    }

    private void ResetMoveTimer()
    {
        _moveTimer = _timeToRoomChange + UnityEngine.Random.Range(0f, _timerVariability);
    }

    private RoomID GetRandomSpawnRoom()
    {
        RoomID[] spawnRooms =
        {
            RoomID.B1F_Cafeteria,
        };

        return spawnRooms[UnityEngine.Random.Range(0, spawnRooms.Length)];
    }

    public bool IsCoachInRoom(RoomID roomId)
    {
        if (_isTransitioning || _currentRoomId == RoomID.None)
            return false;

        return _currentRoomId == roomId;
    }
}