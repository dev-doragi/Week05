using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 코치의 이동 실행과 타이머를 제어합니다. 외부 이벤트 호출을 통해 활성화됩니다.
/// </summary>
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
    [SerializeField] private bool _isActive = false;

    private bool _isTransitioning;
    private bool _isSpawnDelayed = true;
    private float _moveTimer;
    private RoomID _nextRoomId = RoomID.None;
    private Coroutine _moveRoutine;
    private Coroutine _initialDelayRoutine;
    private MapGraph _mapGraph;
    private GimmickManager _gimmickManager;

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
        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        _mapGraph = new MapGraph();

        _currentRoomId = RoomID.None;
        _isSpawnDelayed = true;
        _isActive = false;

        _coachBrain.Initialize(_mapGraph, _gimmickManager);
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += StartMovement;

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
        if (!_isActive || _isTransitioning || _isSpawnDelayed || _currentRoomId == RoomID.Office)
            return;

        _moveTimer -= Time.deltaTime;
        if (_moveTimer <= 0f)
            TryMoveNext();
    }

    public void StartMovement()
    {
        if (_isActive) return;

        _isActive = true;

        if (_initialDelayRoutine != null) StopCoroutine(_initialDelayRoutine);

        _initialDelayRoutine = StartCoroutine(Co_InitialDelay());
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

    private IEnumerator Co_InitialDelay()
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
        RoomID nextRoomId = _coachBrain.GetNextRoom(_currentRoomId);

        if (nextRoomId == _currentRoomId || nextRoomId == RoomID.None)
        {
            ResetMoveTimer();
            return;
        }

        _nextRoomId = nextRoomId;
        _isTransitioning = true;
        OnCoachPreparingToMove?.Invoke(_currentRoomId, _nextRoomId);

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
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

    private void HandleStayDelayActivated()
    {
        if (!_isActive || _isTransitioning || _isSpawnDelayed || _currentRoomId == RoomID.Office)
            return;

        _moveTimer += _gimmickManager.GetExtraStayTime();
    }

    private void ResetMoveTimer()
    {
        _moveTimer = _timeToRoomChange + UnityEngine.Random.Range(0f, _timerVariability);
    }

    private RoomID GetRandomSpawnRoom()
    {
        RoomID[] spawnRooms = { RoomID.B1F_Cafeteria, RoomID.B1F_JungleStepLower, RoomID.B1F_Cafe, };
        return spawnRooms[UnityEngine.Random.Range(0, spawnRooms.Length)];
    }

    public bool IsCoachInRoom(RoomID roomId)
    {
        if (_isTransitioning || _currentRoomId == RoomID.None) return false;
        return _currentRoomId == roomId;
    }
}