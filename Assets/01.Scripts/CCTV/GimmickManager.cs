using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coachController;

    private RoomID _activeTempTarget = RoomID.None;
    private Coroutine _tempTargetRoutine;

    private Dictionary<(RoomID, RoomID), float> _blockedPaths = new();

    private RoomID _activeLureRoom = RoomID.None;
    private float _lureValue = 0f;
    private Coroutine _lureRoutine;

    public event Action<RoomID> OnLureActivated;

    public RoomID ActiveTempTarget => _activeTempTarget;
    public RoomID ActiveLureRoom => _activeLureRoom;

    #region 1. Path Blocking & Elevator Delay

    public void BlockPath(RoomID from, RoomID to, float duration)
    {
        StartCoroutine(Co_BlockPath(from, to, duration));
        StartCoroutine(Co_BlockPath(to, from, duration));
    }

    private IEnumerator Co_BlockPath(RoomID from, RoomID to, float duration)
    {
        var path = (from, to);
        _blockedPaths[path] = Time.time + duration;
        yield return new WaitForSeconds(duration);

        if (_blockedPaths.ContainsKey(path) && _blockedPaths[path] <= Time.time)
        {
            _blockedPaths.Remove(path);
        }
    }

    public void DelayElevator()
    {
        if (_coachController == null) return;

        bool isInsideB1F = _coachController.CurrentRoomId == RoomID.Elevator_B ||
                          _coachController.CurrentRoomId == RoomID.Cafeteria ||
                          _coachController.CurrentRoomId == RoomID.Stair_B;

        bool isMovingTo3F = _coachController.IsTransitioning && _coachController.NextRoomId == RoomID.Elevator_A;

        if (isInsideB1F || isMovingTo3F)
        {
            _coachController.ForceMoveTo(RoomID.Elevator_B);
            Debug.Log("[Gimmick] 엘리베이터 지연 발생: 코치를 B1F로 회귀시킵니다.");
        }
    }

    public bool IsPathBlocked(RoomID from, RoomID to)
    {
        if (_blockedPaths.TryGetValue((from, to), out float unlockTime))
        {
            return Time.time < unlockTime;
        }
        return false;
    }

    #endregion

    #region 2. Temp Target (Interview)

    public void SetTempTarget(RoomID targetRoom, float duration)
    {
        if (_tempTargetRoutine != null) StopCoroutine(_tempTargetRoutine);
        _tempTargetRoutine = StartCoroutine(Co_TempTarget(targetRoom, duration));
    }

    private IEnumerator Co_TempTarget(RoomID targetRoom, float duration)
    {
        _activeTempTarget = targetRoom;
        yield return new WaitForSeconds(duration);
        _activeTempTarget = RoomID.None;
        _tempTargetRoutine = null;
    }

    #endregion

    #region 3. Sound Lure

    public void ActivateLure(RoomID room, float initialLureValue, float duration)
    {
        if (_lureRoutine != null) StopCoroutine(_lureRoutine);
        _lureRoutine = StartCoroutine(Co_DecayLure(room, initialLureValue, duration));

        OnLureActivated?.Invoke(room);
    }

    private IEnumerator Co_DecayLure(RoomID room, float maxValue, float duration)
    {
        _activeLureRoom = room;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _lureValue = Mathf.Lerp(maxValue, 0f, elapsed / duration);
            yield return null;
        }
        _activeLureRoom = RoomID.None;
        _lureValue = 0f;
        _lureRoutine = null;
    }

    public float GetLureValue(RoomID room)
    {
        return (room == _activeLureRoom) ? _lureValue : 0f;
    }

    #endregion
}