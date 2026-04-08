using System;
using System.Collections;
using UnityEngine;

public class CoachMovementController : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _timeToRoomChange = 8f;
    [SerializeField] private float _timerVariability = 2f;
    [SerializeField] private float _transitionDuration = 2f;
    [SerializeField] private bool _playOnStart = true;

    [Header("Dependencies")]
    [SerializeField] private GimmickManager _gimmickManager;
    [SerializeField] private CoachBrain _coachBrain; // 인스펙터에서 Aggro 세팅 조정 가능

    [Header("Debug")]
    [SerializeField] private RoomID _currentRoomId = RoomID.None;

    private bool _isTransitioning;
    private float _moveTimer;
    private RoomID _nextRoomId = RoomID.None;
    private Coroutine _moveRoutine;
    private MapGraph _mapGraph;

    public RoomID CurrentRoomId => _currentRoomId;
    public RoomID NextRoomId => _nextRoomId;
    public bool IsTransitioning => _isTransitioning;
    public float TransitionDuration => _transitionDuration;

    public event Action<RoomID, RoomID> OnCoachMoved;
    public event Action<RoomID, RoomID> OnCoachPreparingToMove;
    public event Action OnCoachReachedOffice;

    public float CurrentAggro => _coachBrain != null ? _coachBrain.CurrentAggro : 0f;

    private void Awake()
    {
        // 맵 그래프 초기화
        _mapGraph = new MapGraph();

        // GimmickManager가 없으면 현재 게임오브젝트에서 탐색 시도
        if (_gimmickManager == null)
            _gimmickManager = FindAnyObjectByType<GimmickManager>();

        // 두뇌 초기화
        _coachBrain.Initialize(_mapGraph, _gimmickManager);

        if (_currentRoomId == RoomID.None)
            _currentRoomId = GetRandomSpawnRoom();

        ResetMoveTimer();
    }

    private void Start()
    {
        if (!_playOnStart)
            return;

        StartMovement();
    }

    private void Update()
    {
        if (!_playOnStart || _isTransitioning || _currentRoomId == RoomID.Office)
            return;

        _moveTimer -= Time.deltaTime;

        if (_moveTimer <= 0f)
            TryMoveNext();
    }

    public bool IsCoachInRoom(RoomID roomId)
    {
        if (_isTransitioning) return false;
        return _currentRoomId == roomId;
    }

    public void StartMovement()
    {
        _playOnStart = true;
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
        // CoachBrain을 통해 다음 방 결정
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

        // 이동 완료 후 어그로 증가
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
        RoomID[] spawnRooms =
        {
            RoomID.Cafeteria,
            RoomID.Elevator_B,
            RoomID.Stair_B
        };

        return spawnRooms[UnityEngine.Random.Range(0, spawnRooms.Length)];
    }
}