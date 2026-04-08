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
            case RoomID.Cafeteria:
                return UnityEngine.Random.value < 0.6f ? RoomID.JungleStep : RoomID.Elevator_B;

            case RoomID.JungleStep:
                return UnityEngine.Random.value < 0.65f ? RoomID.Lobby : RoomID.Cafeteria;

            case RoomID.Lobby:
                return UnityEngine.Random.value < 0.5f ? RoomID.Stair_A : RoomID.Stair_B;

            case RoomID.Stair_A:
                return UnityEngine.Random.value < 0.5f ? RoomID.LeftHallwayNearOffice : RoomID.RightHallwayNearOffice;

            case RoomID.Stair_B:
                return UnityEngine.Random.value < 0.5f ? RoomID.LeftHallwayNearOffice : RoomID.RightHallwayNearOffice;

            case RoomID.Lounge:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.4f) return RoomID.LeftHallwayNearOffice;
                    if (roll < 0.8f) return RoomID.RightHallwayNearOffice;
                    return UnityEngine.Random.value < 0.5f ? RoomID.Elevator_A : RoomID.Elevator_B;
                }

            case RoomID.Elevator_A:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.4f) return RoomID.Lounge;
                    if (roll < 0.7f) return RoomID.LeftHallwayNearOffice;
                    return RoomID.RightHallwayNearOffice;
                }

            case RoomID.Elevator_B:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.3f) return RoomID.Cafeteria;
                    if (roll < 0.6f) return RoomID.Lounge;
                    if (roll < 0.8f) return RoomID.LeftHallwayNearOffice;
                    return RoomID.RightHallwayNearOffice;
                }

            case RoomID.LeftHallwayNearOffice:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.65f) return RoomID.Office;
                    if (roll < 0.85f) return RoomID.RightHallwayNearOffice;
                    return RoomID.Lounge;
                }

            case RoomID.RightHallwayNearOffice:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.65f) return RoomID.Office;
                    if (roll < 0.85f) return RoomID.LeftHallwayNearOffice;
                    return RoomID.Lounge;
                }

            case RoomID.Office:
                return RoomID.Office;
        }

        return RoomID.Office;
    }
}