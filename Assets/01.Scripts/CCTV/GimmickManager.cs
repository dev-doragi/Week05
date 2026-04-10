using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coachController;

    [Header("BlockPathCoolDown")]
    [SerializeField] private float _blockPathDuration = 5f;
    [SerializeField] private float _blockPathCooldown = 10f;

    private RoomID _activeTempTarget = RoomID.None;
    private Coroutine _tempTargetRoutine;

    private RoomID _activeLureRoom = RoomID.None;
    private float _lureValue = 0f;
    private Coroutine _lureRoutine;

    public event Action<RoomID> OnLureActivated;

    public RoomID ActiveTempTarget => _activeTempTarget;
    public RoomID ActiveLureRoom => _activeLureRoom;

    #region 1. Path Blocking

    private bool _hasBlockedPath;
    private RoomID _blockedFrom = RoomID.None;
    private RoomID _blockedTo = RoomID.None;
    private float _blockPathEndTime = 0f;
    private float _nextBlockPathAvailableTime = 0f;

    public bool HasBlockedPath => _hasBlockedPath && Time.time < _blockPathEndTime;
    public bool CanUseBlockPath => Time.time >= _nextBlockPathAvailableTime;

    public void BlockPath(RoomID from, RoomID to)
    {
        _hasBlockedPath = true;
        _blockedFrom = from;
        _blockedTo = to;
        _blockPathEndTime = Time.time + _blockPathDuration;
        _nextBlockPathAvailableTime = Time.time + _blockPathCooldown;
    }

    public void ClearBlockedPath()
    {
        _hasBlockedPath = false;
        _blockedFrom = RoomID.None;
        _blockedTo = RoomID.None;
        _blockPathEndTime = 0f;
    }

    public bool IsPathBlocked(RoomID from, RoomID to)
    {
        if (_hasBlockedPath == false)
            return false;

        if (Time.time >= _blockPathEndTime)
        {
            ClearBlockedPath();
            return false;
        }

        return (_blockedFrom == from && _blockedTo == to)
            || (_blockedFrom == to && _blockedTo == from);
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