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
    private Coroutine _moveRoutine;

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
        RoomID nextRoomId = GetNextRoom(_currentRoomId);

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

        int index = UnityEngine.Random.Range(0, spawnRooms.Length);
        return spawnRooms[index];
    }

    private RoomID GetNextRoom(RoomID currentRoomId)
    {
        switch (currentRoomId)
        {
            case RoomID.Cafeteria:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.65f) return RoomID.JungleStep;
                    return RoomID.Elevator_B;
                }

            case RoomID.JungleStep:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.7f) return RoomID.Cafeteria;
                    return RoomID.Lobby;
                }

            case RoomID.Lobby:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.45f) return RoomID.JungleStep;
                    if (roll < 0.8f) return RoomID.Stair_B;
                    return RoomID.Elevator_B;
                }

            // Stair_A(3층 계단) -> 라운지 or 엘리베이터
            case RoomID.Stair_A:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.6f) return RoomID.Lounge;
                    return RoomID.Elevator_A;
                }

            // Stair_B(1층 계단) -> 정글 스텝 or 로비 or 3층 계단
            case RoomID.Stair_B:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.4f) return RoomID.JungleStep;
                    if (roll < 0.75f) return RoomID.Lobby;
                    return RoomID.Stair_A;
                }

            // 라운지 -> 코칭룸 or 3층 계단, 낮은 확률로 니어오피스로
            case RoomID.Lounge:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.35f) return RoomID.CoachingRoom;
                    if (roll < 0.7f) return RoomID.Stair_A;
                    if (roll < 0.85f) return RoomID.LeftHallwayNearOffice;
                    return RoomID.RightHallwayNearOffice;
                }

            // 코칭룸 -> 라운지 or 3층 엘베
            case RoomID.CoachingRoom:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.5f) return RoomID.Lounge;
                    if (roll < 0.7f) return RoomID.LeftHallwayNearOffice;
                    if (roll < 0.85f) return RoomID.Lounge;
                    return RoomID.Elevator_A;
                }

            // 3층 엘베 -> 낮은 확률로 b1f 엘베 or 오른쪽 복도, 낮은 확률로 라운지
            case RoomID.Elevator_A:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.5f) return RoomID.Stair_A;
                    if (roll < 0.7f) return RoomID.RightHallwayNearOffice;
                    if (roll < 0.85f) return RoomID.Lounge;
                    return RoomID.Elevator_B;
                }

            // b1f 엘베 -> 낮은 확률로 3층 엘베 or 카페테리아 or 낮은 확률로 1층 계단
            case RoomID.Elevator_B:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.6f) return RoomID.Cafeteria;
                    if (roll < 0.8f) return RoomID.Elevator_A;
                    return RoomID.Stair_B;
                }

            // 왼쪽 오른쪽 왔다갔다 + 안막으면 결국 오피스 들어옴 + 낮은 확률로 라운지
            case RoomID.LeftHallwayNearOffice:
                {
                    float roll = UnityEngine.Random.value;
                    if (roll < 0.65f) return RoomID.Office;
                    if (roll < 0.85f) return RoomID.RightHallwayNearOffice;
                    return RoomID.Lounge;
                }

            // 왼쪽 오른쪽 왔다갔다 + 안막으면 결국 오피스 들어옴 + 낮은 확률로 라운지
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