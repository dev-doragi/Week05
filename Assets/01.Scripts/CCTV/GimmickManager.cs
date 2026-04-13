using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class GimmickManager : Singleton<GimmickManager>
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coachController;

    [Header("BlockPathCoolDown")]
    [SerializeField] private float _blockPathDuration = 5f;
    [SerializeField] private float _blockPathCooldown = 10f;

    private RoomID _activeTempTarget = RoomID.None;
    private Coroutine _tempTargetRoutine;

    private Coroutine _stayDelayRoutine;
    private float _activeExtraStayTime;
    private float _stayDelayEndTime;
    private GimmickType _activeStayDelayType = GimmickType.None;

    private bool _hasBlockedPath;
    private RoomID _blockedFrom = RoomID.None;
    private RoomID _blockedTo = RoomID.None;
    private float _blockPathEndTime;
    private float _nextBlockPathAvailableTime;

    public float BlockPathCooldownDuration => _blockPathCooldown;
    public RoomID ActiveTempTarget => _activeTempTarget;
    public bool IsBlockPathCooldownReady => Time.time >= _nextBlockPathAvailableTime;
    public float BlockPathCooldownRemaining => Mathf.Max(0f, _nextBlockPathAvailableTime - Time.time);
    public bool HasBlockedPath => _hasBlockedPath && Time.time < _blockPathEndTime;
    public bool HasActiveStayDelay => Time.time < _stayDelayEndTime;
    public float ActiveExtraStayTime => HasActiveStayDelay ? _activeExtraStayTime : 0f;
    public GimmickType ActiveStayDelayType => HasActiveStayDelay ? _activeStayDelayType : GimmickType.None;

    public event Action<GimmickType> OnGimmickActivated;
    public event Action<GimmickType> OnStayDelayActivated;
    public event Action<RoomID, RoomID> OnPathBlocked;
    public event Action OnFakeInterviewRevealed;

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

    public bool CanUseGimmick
    {
        get
        {
            if (_coachController != null && _coachController.IsTransitioning)
                return false;

            return true;
        }
    }

    #region 1. Path Blocking

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

        ToastManager.Instance.SendBlockedPathNoticeToast(from, to);

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

    #region 2. Temp Target

    public bool SetTempTarget(RoomID targetRoom, float duration)
    {
        if (CanUseGimmick == false)
            return false;

        if (_tempTargetRoutine != null)
            StopCoroutine(_tempTargetRoutine);

        _tempTargetRoutine = StartCoroutine(CoTempTarget(targetRoom, duration));
        OnGimmickActivated?.Invoke(GimmickType.RequestInterview);
        return true;
    }

    private IEnumerator CoTempTarget(RoomID targetRoom, float duration)
    {
        _activeTempTarget = targetRoom;

        if (targetRoom == RoomID.F3_CoachingRoom)
        {
            while (_coachController != null && _coachController.CurrentRoomId != RoomID.F3_CoachingRoom)
                yield return null;

            ActivateStayDelay(GimmickType.RequestInterview, RoomID.F3_CoachingRoom, duration, duration);

            yield return new WaitForSeconds(duration);

            RevealFakeInterview();

            if (_coachController != null && _coachController.CoachBrain != null)
                _coachController.CoachBrain.SetAggro(1f);

            _tempTargetRoutine = null;
            yield break;
        }

        yield return new WaitForSeconds(duration);

        _activeTempTarget = RoomID.None;
        _tempTargetRoutine = null;
    }

    public void RevealFakeInterview()
    {
        _activeTempTarget = RoomID.None;
        _tempTargetRoutine = null;
        OnFakeInterviewRevealed?.Invoke();
    }

    #endregion

    #region 3. Stay Delay

    public bool ActivateStayDelay(GimmickType gimmickType, RoomID targetRoom, float extraStayTime, float duration)
    {
        if (CanUseGimmick == false)
            return false;

        if (_coachController == null)
            return false;

        if (_coachController.CurrentRoomId != targetRoom)
            return false;

        if (_stayDelayRoutine != null)
            StopCoroutine(_stayDelayRoutine);

        _stayDelayRoutine = StartCoroutine(CoStayDelay(gimmickType, extraStayTime, duration));

        OnGimmickActivated?.Invoke(gimmickType);
        OnStayDelayActivated?.Invoke(gimmickType);
        return true;
    }

    private IEnumerator CoStayDelay(GimmickType gimmickType, float extraStayTime, float duration)
    {
        _activeStayDelayType = gimmickType;
        _activeExtraStayTime = extraStayTime;
        _stayDelayEndTime = Time.time + duration;

        yield return new WaitForSeconds(duration);

        _activeStayDelayType = GimmickType.None;
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