using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    private RoomID _activeTempTarget = RoomID.None;
    private Coroutine _tempTargetRoutine;

    // 차단된 경로와 해제 시간을 저장
    private Dictionary<(RoomID, RoomID), float> _blockedPaths = new();

    private RoomID _activeLureRoom = RoomID.None;
    private float _lureValue = 0f;
    private Coroutine _lureRoutine;

    public RoomID ActiveTempTarget => _activeTempTarget;

    // 1. 엘리베이터 조작 등 (특정 경로 일정 시간 차단)
    public void BlockPath(RoomID from, RoomID to, float duration)
    {
        StartCoroutine(Co_BlockPath(from, to, duration));
        StartCoroutine(Co_BlockPath(to, from, duration)); // 양방향 차단 적용
    }

    private IEnumerator Co_BlockPath(RoomID from, RoomID to, float duration)
    {
        Debug.Log("엘리베이터 버튼을 눌렀다.");
        var path = (from, to);
        _blockedPaths[path] = Time.time + duration;

        yield return new WaitForSeconds(duration);

        if (_blockedPaths.ContainsKey(path) && _blockedPaths[path] <= Time.time)
        {
            _blockedPaths.Remove(path);
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

    // 2. 면담 신청 (일정 시간 동안 목표 변경)
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

    // 3. 소리 유인 (해당 방의 점수를 일시적으로 높이고 서서히 감소)
    public void ActivateLure(RoomID room, float initialLureValue, float duration)
    {
        if (_lureRoutine != null)
            StopCoroutine(_lureRoutine);

        _lureRoutine = StartCoroutine(Co_DecayLure(room, initialLureValue, duration));
    }

    private IEnumerator Co_DecayLure(RoomID room, float maxValue, float duration)
    {
        _activeLureRoom = room;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 시간에 따라 유인 수치 선형 감소
            _lureValue = Mathf.Lerp(maxValue, 0f, elapsed / duration);
            yield return null;
        }

        _activeLureRoom = RoomID.None;
        _lureValue = 0f;
        _lureRoutine = null;
    }

    public float GetLureValue(RoomID room)
    {
        if (room == _activeLureRoom)
            return _lureValue;

        return 0f;
    }
}