using System;
using System.Collections;
using UnityEngine;

public class VignetteFloor : MonoBehaviour
{
    private enum CoachFloor
    {
        None,
        B1,
        F1,
        F3,
        Office
    }

    [Serializable]
    private struct FloorPulseStage
    {
        public CoachFloor floor;
        public VignetteEffectSettings pulseSettings;
        [Min(0f)] public float delayBetweenCycles;
    }

    [Header("References")]
    [SerializeField] private CoachMovementController _coachMovementController;
    [SerializeField] private VignetteService _vignetteService;

    [Header("Default")]
    [SerializeField][Range(0f, 1f)] private float _defaultIntensity = 0f;
    [SerializeField] private Color _defaultColor = Color.black;

    [Header("Floor Pulse")]
    [SerializeField] private FloorPulseStage[] _floorStages;

    private Coroutine _pulseRoutine;
    private FloorPulseStage _activeStage;
    private CoachFloor _activeFloor = CoachFloor.None;

    private void Awake()
    {
        if (_coachMovementController == null)
            _coachMovementController = FindFirstObjectByType<CoachMovementController>();
    }

    private void OnEnable()
    {
        if (_coachMovementController != null)
            _coachMovementController.OnCoachMoved += HandleCoachMoved;

        RefreshPulseState();
    }

    private void OnDisable()
    {
        if (_coachMovementController != null)
            _coachMovementController.OnCoachMoved -= HandleCoachMoved;

        StopPulse();
        ApplyDefault();
    }

    private void OnDestroy()
    {
        StopPulse();
    }

    private void HandleCoachMoved(RoomID from, RoomID to)
    {
        RefreshPulseState();
    }

    private void RefreshPulseState()
    {
        CoachFloor currentFloor = GetCurrentCoachFloor();

        if (currentFloor == CoachFloor.None ||
            currentFloor == CoachFloor.B1 ||
            currentFloor == CoachFloor.Office)
        {
            StopPulse();
            ApplyDefault();
            return;
        }

        if (!TryGetStage(currentFloor, out FloorPulseStage nextStage))
        {
            StopPulse();
            ApplyDefault();
            return;
        }

        if (_pulseRoutine != null && _activeFloor == currentFloor)
            return;

        StartPulse(currentFloor, nextStage);
    }

    private bool TryGetStage(CoachFloor floor, out FloorPulseStage stage)
    {
        stage = default;

        if (_floorStages == null || _floorStages.Length == 0)
            return false;

        foreach (FloorPulseStage candidate in _floorStages)
        {
            if (candidate.floor != floor)
                continue;

            stage = candidate;
            return true;
        }

        return false;
    }

    private void StartPulse(CoachFloor floor, FloorPulseStage stage)
    {
        StopPulse();

        _activeFloor = floor;
        _activeStage = stage;
        _pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private void StopPulse()
    {
        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }

        _activeFloor = CoachFloor.None;
    }

    private void ApplyDefault()
    {
        _vignetteService?.SetImmediate(_defaultIntensity, _defaultColor);
    }

    private IEnumerator PulseRoutine()
    {
        while (IsActiveFloorStillValid())
        {
            _vignetteService.Play(_activeStage.pulseSettings);

            float waitTime = Mathf.Max(
                _activeStage.pulseSettings.duration,
                _activeStage.delayBetweenCycles
            );

            yield return new WaitForSeconds(waitTime);
        }

        _pulseRoutine = null;
    }

    private bool IsActiveFloorStillValid()
    {
        return GetCurrentCoachFloor() == _activeFloor;
    }

    private CoachFloor GetCurrentCoachFloor()
    {
        if (_coachMovementController == null)
            return CoachFloor.None;

        return ConvertRoomToFloor(_coachMovementController.CurrentRoomId);
    }

    private CoachFloor ConvertRoomToFloor(RoomID roomId)
    {
        string roomName = roomId.ToString();

        if (roomName.StartsWith("B1F")) return CoachFloor.B1;
        if (roomName.StartsWith("F1")) return CoachFloor.F1;
        if (roomName.StartsWith("F3")) return CoachFloor.F3;
        if (roomId == RoomID.Office) return CoachFloor.Office;

        return CoachFloor.None;
    }
}
