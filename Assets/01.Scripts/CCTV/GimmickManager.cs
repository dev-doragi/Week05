using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GimmickManager : Singleton<GimmickManager>
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coachController;

    [Header("BlockPathCoolDown")]
    [SerializeField] private float _blockPathDuration = 5f;
    [SerializeField] private float _blockPathCooldown = 10f;
    public float BlockPathCooldownDuration => _blockPathCooldown;
    private RoomID _activeTempTarget = RoomID.None;
    private Coroutine _tempTargetRoutine;

    private Coroutine _stayDelayRoutine;
    private float _activeExtraStayTime = 0f;
    private float _stayDelayEndTime = 0f;

    public event Action OnStayDelayActivated;
    public event Action<RoomID, RoomID> OnPathBlocked;

    public RoomID ActiveTempTarget => _activeTempTarget;

    public bool IsBlockPathCooldownReady => Time.time >= _nextBlockPathAvailableTime;
    public float BlockPathCooldownRemaining => Mathf.Max(0f, _nextBlockPathAvailableTime - Time.time);

    #region 1. Path Blocking

    private bool _hasBlockedPath;
    private RoomID _blockedFrom = RoomID.None;
    private RoomID _blockedTo = RoomID.None;
    private float _blockPathEndTime = 0f;
    private float _nextBlockPathAvailableTime = 0f;

    public bool HasBlockedPath => _hasBlockedPath && Time.time < _blockPathEndTime;

    protected override void Init()
    {
    }

    public bool CanUseBlockPath
    {
        get
        {
            if (Time.time < _nextBlockPathAvailableTime)
                return false;

            if (_coachController != null && _coachController.IsTransitioning)
                return false;

            return true;
        }
    }

    public bool TryBlockPath(RoomID from, RoomID to)
    {
        if (CanUseBlockPath == false)
            return false;

        BlockPath(from, to);
        return true;
    }

    public void BlockPath(RoomID from, RoomID to)
    {
        _hasBlockedPath = true;
        _blockedFrom = from;
        _blockedTo = to;
        _blockPathEndTime = Time.time + _blockPathDuration;
        _nextBlockPathAvailableTime = Time.time + _blockPathCooldown;

        OnPathBlocked?.Invoke(from, to);
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
        if (_tempTargetRoutine != null)
            StopCoroutine(_tempTargetRoutine);

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

    #region 3. Stay Delay (Sound Lure)

    public bool HasActiveStayDelay => Time.time < _stayDelayEndTime;
    public float ActiveExtraStayTime => HasActiveStayDelay ? _activeExtraStayTime : 0f;

    public bool ActivateStayDelay(RoomID targetRoom, float extraStayTime, float duration)
    {
        if (_coachController == null)
            return false;

        if (_coachController.CurrentRoomId != targetRoom)
            return false;

        if (_stayDelayRoutine != null)
            StopCoroutine(_stayDelayRoutine);

        _stayDelayRoutine = StartCoroutine(Co_StayDelay(extraStayTime, duration));
        OnStayDelayActivated?.Invoke();
        return true;
    }

    private IEnumerator Co_StayDelay(float extraStayTime, float duration)
    {
        _activeExtraStayTime = extraStayTime;
        _stayDelayEndTime = Time.time + duration;

        yield return new WaitForSeconds(duration);

        _activeExtraStayTime = 0f;
        _stayDelayEndTime = 0f;
        _stayDelayRoutine = null;
    }

    public float GetExtraStayTime()
    {
        if (HasActiveStayDelay == false)
            return 0f;

        return _activeExtraStayTime;
    }

    #endregion
}