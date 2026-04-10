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

    private Coroutine _stayDelayRoutine;
    private float _activeExtraStayTime = 0f;
    private float _stayDelayEndTime = 0f;

    public event Action OnStayDelayActivated;

    public RoomID ActiveTempTarget => _activeTempTarget;

    public static GimmickManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region 1. Path Blocking

    private bool _hasBlockedPath;
    private RoomID _blockedFrom = RoomID.None;
    private RoomID _blockedTo = RoomID.None;
    private float _blockPathEndTime = 0f;
    private float _nextBlockPathAvailableTime = 0f;

    public bool HasBlockedPath => _hasBlockedPath && Time.time < _blockPathEndTime;

    public bool CanUseBlockPath
    {
        get
        {
            if (Time.time < _nextBlockPathAvailableTime)
                return false;

            if (_coachController.IsTransitioning)
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

    #region 3. Stay Delay (Sound Lure)

    public bool HasActiveStayDelay => Time.time < _stayDelayEndTime;
    public float ActiveExtraStayTime => HasActiveStayDelay ? _activeExtraStayTime : 0f;

    public void ActivateStayDelay(float extraStayTime, float duration)
    {
        if (_stayDelayRoutine != null)
            StopCoroutine(_stayDelayRoutine);

        _stayDelayRoutine = StartCoroutine(Co_StayDelay(extraStayTime, duration));
        OnStayDelayActivated?.Invoke();
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