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

    [Header("Debug")]
    [SerializeField] private RoomID _currentRoomId = RoomID.None;

    private bool _isTransitioning;
    private float _moveTimer;
    private RoomID _nextRoomId = RoomID.None;

    public RoomID CurrentRoomId => _currentRoomId;
    public RoomID NextRoomId => _nextRoomId;
    public bool IsTransitioning => _isTransitioning;

    public float TransitionDuration => _transitionDuration;

    public event Action<RoomID, RoomID> OnCoachMoved;
    public event Action<RoomID, RoomID> OnCoachPreparingToMove;
    public event Action OnCoachReachedOffice;

    private void Awake()
    {
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
        if (!_playOnStart)
            return;

        if (_isTransitioning)
            return;

        if (_currentRoomId == RoomID.Office)
            return;

        _moveTimer -= Time.deltaTime;

        if (_moveTimer > 0f)
            return;

        TryMoveNext();
    }

    public bool IsCoachInRoom(RoomID roomId)
    {
        if (_isTransitioning)
            return false;

        return _currentRoomId == roomId;
    }

    public void StartMovement()
    {
        _playOnStart = true;
    }

    public void StopMovement()
    {
        _playOnStart = false;
    }

    public void ForceMoveTo(RoomID targetRoomId)
    {
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
        RoomID nextRoomId = GetNextRoom(_currentRoomId);

        if (nextRoomId == _currentRoomId || nextRoomId == RoomID.None)
        {
            ResetMoveTimer();
            return;
        }

        _nextRoomId = nextRoomId;
        _isTransitioning = true;
        OnCoachPreparingToMove?.Invoke(_currentRoomId, _nextRoomId);
        StartCoroutine(Co_MoveAfterDelay());
    }

    private IEnumerator Co_MoveAfterDelay()
    {
        yield return new WaitForSeconds(_transitionDuration);

        RoomID previousRoomId = _currentRoomId;
        _currentRoomId = _nextRoomId;
        _nextRoomId = RoomID.None;
        _isTransitioning = false;

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
            RoomID.CoachingRoom,
            RoomID.Cafeteria,
            RoomID.Elevator_B,
            RoomID.Stair_B
        };

        int index = UnityEngine.Random.Range(0, spawnRooms.Length);
        return spawnRooms[index];
    }

    private RoomID GetNextRoom(RoomID currentRoomId)
    {
        switch (currentRoomId)
        {
            case RoomID.CoachingRoom:
                return RoomID.Elevator_B;

            case RoomID.Cafeteria:
                return UnityEngine.Random.value < 0.5f ? RoomID.Lobby : RoomID.Elevator_B;

            case RoomID.Lounge:
                return UnityEngine.Random.value < 0.5f ? RoomID.Stair_B : RoomID.Lobby;

            case RoomID.Lobby:
                return UnityEngine.Random.value < 0.5f ? RoomID.Stair_A : RoomID.Stair_B;

            case RoomID.JungleStep:
                return UnityEngine.Random.value < 0.5f ? RoomID.Stair_A : RoomID.Lobby;

            case RoomID.Elevator_B:
                return UnityEngine.Random.value < 0.5f ? RoomID.Lounge : RoomID.Lobby;

            case RoomID.Elevator_A:
                return UnityEngine.Random.value < 0.5f ? RoomID.LeftHallwayNearOffice : RoomID.RightHallwayNearOffice;

            case RoomID.Stair_B:
                return UnityEngine.Random.value < 0.5f ? RoomID.LeftHallwayNearOffice : RoomID.RightHallwayNearOffice;

            case RoomID.Stair_A:
                return UnityEngine.Random.value < 0.5f ? RoomID.LeftHallwayNearOffice : RoomID.RightHallwayNearOffice;

            case RoomID.LeftHallwayNearOffice:
                return UnityEngine.Random.value < 0.7f ? RoomID.Office : RoomID.RightHallwayNearOffice;

            case RoomID.RightHallwayNearOffice:
                return UnityEngine.Random.value < 0.7f ? RoomID.Office : RoomID.LeftHallwayNearOffice;

            case RoomID.Office:
                return RoomID.Office;

            default:
                return currentRoomId;
        }
    }
}