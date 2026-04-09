using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 코치의 이동 실행과 타이머를 제어합니다. 초기 지연 후 스폰 로직이 포함되어 있습니다.
/// </summary>
public class CoachMovementController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _initialSpawnDelay = 5f;
    [SerializeField] private float _timeToRoomChange = 8f;
    [SerializeField] private float _timerVariability = 2f;
    [SerializeField] private float _transitionDuration = 2f;
    [SerializeField] private bool _playOnStart = false;

    [Header("Dependencies")]
    [SerializeField] private GimmickManager _gimmickManager;
    [SerializeField] private CoachBrain _coachBrain;

    [Header("Debug")]
    [SerializeField] private RoomID _currentRoomId = RoomID.None;

    private bool _isTransitioning;
    private bool _isSpawnDelayed = true;
    private float _moveTimer;
    private RoomID _nextRoomId = RoomID.None;
    private Coroutine _moveRoutine;
    private MapGraph _mapGraph;

    public RoomID CurrentRoomId => _currentRoomId;
    public RoomID NextRoomId => _nextRoomId;

    public MapGraph MapGraph => _mapGraph;
    public bool IsTransitioning => _isTransitioning;
    public float TransitionDuration => _transitionDuration;

    public event Action<RoomID, RoomID> OnCoachMoved;
    public event Action<RoomID, RoomID> OnCoachPreparingToMove;
    public event Action OnCoachReachedOffice;

    public float CurrentAggro => _coachBrain != null ? _coachBrain.CurrentAggro : 0f;

    private void Awake()
    {
        _mapGraph = new MapGraph();
        if (_gimmickManager == null)
            _gimmickManager = FindAnyObjectByType<GimmickManager>();

        _coachBrain.Initialize(_mapGraph, _gimmickManager);

        // 초기에는 None 상태로 시작 (CCTV에 보이지 않음)
        _currentRoomId = RoomID.None;
        _isSpawnDelayed = true;
    }

    private void Start()
    {
        if (!_playOnStart) return;
        StartCoroutine(Co_InitialDelay());
    }

    private void Update()
    {
        if (!_playOnStart || _isTransitioning || _isSpawnDelayed || _currentRoomId == RoomID.Office)
            return;

        _moveTimer -= Time.deltaTime;
        if (_moveTimer <= 0f)
            TryMoveNext();
    }

    public bool IsCoachInRoom(RoomID roomId)
    {
        if (_isTransitioning || _currentRoomId == RoomID.None) return false;
        return _currentRoomId == roomId;
    }

    /// <summary>
    /// 지정된 지연 시간 이후 첫 스폰 위치를 결정하고 활동을 시작합니다.
    /// </summary>
    private IEnumerator Co_InitialDelay()
    {
        yield return new WaitForSeconds(_initialSpawnDelay);

        _currentRoomId = GetRandomSpawnRoom();
        _isSpawnDelayed = false;
        ResetMoveTimer();

        OnCoachMoved?.Invoke(RoomID.None, _currentRoomId);
    }

    public void StartMovement()
    {
        _playOnStart = true;
        if (_currentRoomId == RoomID.None)
        {
            StopAllCoroutines();
            _currentRoomId = GetRandomSpawnRoom();
            OnCoachMoved?.Invoke(RoomID.None, _currentRoomId);
        }
        _isSpawnDelayed = false;
        ResetMoveTimer();
    }

    public void StopMovement()
    {
        _playOnStart = false;
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
        _isTransitioning = false;
        _nextRoomId = RoomID.None;
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

        ResetMoveTimer();
        OnCoachMoved?.Invoke(previousRoomId, _currentRoomId);

        if (_currentRoomId == RoomID.Office)
            OnCoachReachedOffice?.Invoke();
    }

    private void TryMoveNext()
    {
        RoomID nextRoomId = _coachBrain.GetNextRoom(_currentRoomId);

        if (nextRoomId == _currentRoomId || nextRoomId == RoomID.None)
        {
            ResetMoveTimer();
            return;
        }

        _nextRoomId = nextRoomId;
        _isTransitioning = true;
        OnCoachPreparingToMove?.Invoke(_currentRoomId, _nextRoomId);

        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);

        _moveRoutine = StartCoroutine(Co_MoveAfterDelay());
    }

    private IEnumerator Co_MoveAfterDelay()
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

    private void ResetMoveTimer()
    {
        _moveTimer = _timeToRoomChange + UnityEngine.Random.Range(0f, _timerVariability);
    }

    private RoomID GetRandomSpawnRoom()
    {
        RoomID[] spawnRooms = { RoomID.Cafeteria, RoomID.Elevator_B, RoomID.Stair_B };
        return spawnRooms[UnityEngine.Random.Range(0, spawnRooms.Length)];
    }
}