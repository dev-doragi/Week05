using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_WarningIndicator : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coach;
    [SerializeField] private Image _warningImage;

    [Header("Geiger Settings")]
    [Range(0f, 1f)][SerializeField] private float _baseNoiseLevel = 0.05f;
    [SerializeField] private float _maxDistance = 5f;
    [SerializeField] private float _minAlpha = 0.1f;
    [SerializeField] private float _maxAlpha = 0.8f;

    [Header("Floor Multipliers")]
    [SerializeField] private float _firstFloorMultiplier = 0.3f;
    [SerializeField] private float _thirdFloorMultiplier = 1f;

    [Header("Timing")]
    [SerializeField] private float _tickInterval = 0.02f;

    private Coroutine _geigerRoutine;
    private float _currentIntensity = 0f;
    private bool _forceSolid;

    private void OnEnable()
    {
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += HandleCoachPreparingToMove;
            _coach.OnCoachMoved += HandleCoachMoved;
        }

        StartGeigerCounter();
        UpdateIntensity(_coach != null ? _coach.CurrentRoomId : RoomID.None);
    }

    private void OnDisable()
    {
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove -= HandleCoachPreparingToMove;
            _coach.OnCoachMoved -= HandleCoachMoved;
        }

        StopGeigerCounter();
    }

    private void HandleCoachPreparingToMove(RoomID from, RoomID to)
    {
        _forceSolid = (to == RoomID.Office);
        UpdateIntensity(to);
    }

    private void HandleCoachMoved(RoomID from, RoomID to)
    {
        _forceSolid = false;
        UpdateIntensity(_coach.CurrentRoomId);
    }

    private void UpdateIntensity(RoomID currentRoom)
    {
        if (_coach == null || _coach.MapGraph == null || currentRoom == RoomID.None)
        {
            _currentIntensity = 0f;
            return;
        }

        if (IsBasement(currentRoom))
        {
            _currentIntensity = 0f;
            return;
        }

        if (currentRoom == RoomID.Office)
        {
            _currentIntensity = 1f;
            return;
        }

        int distance = _coach.MapGraph.GetDistance(currentRoom, RoomID.Office);

        float distanceIntensity;
        if (distance <= 0)
            distanceIntensity = 1f;
        else
            distanceIntensity = Mathf.Clamp01(1f - ((float)distance / _maxDistance));

        float floorMultiplier = GetFloorMultiplier(currentRoom);
        _currentIntensity = Mathf.Clamp01(distanceIntensity * floorMultiplier);
    }

    private float GetFloorMultiplier(RoomID room)
    {
        if (IsBasement(room))
            return 0f;

        if (IsFirstFloor(room))
            return _firstFloorMultiplier;

        if (IsThirdFloor(room))
            return _thirdFloorMultiplier;

        if (room == RoomID.Office)
            return 1f;

        return 0f;
    }

    private bool IsBasement(RoomID room)
    {
        switch (room)
        {
            case RoomID.B1F_Cafeteria:
            case RoomID.B1F_JungleStepLower:
            case RoomID.B1F_Cafe:
            case RoomID.B1F_Stair:
                return true;
        }

        return false;
    }

    private bool IsFirstFloor(RoomID room)
    {
        switch (room)
        {
            case RoomID.F1_Elevator:
            case RoomID.F1_JungleStepUpper:
            case RoomID.F1_Lobby:
            case RoomID.F1_Stair:
                return true;
        }

        return false;
    }

    private bool IsThirdFloor(RoomID room)
    {
        switch (room)
        {
            case RoomID.F3_Elevator:
            case RoomID.F3_Lounge:
            case RoomID.F3_CoachingRoom:
            case RoomID.F3_Hallway:
                return true;
        }

        return false;
    }

    private void StartGeigerCounter()
    {
        StopGeigerCounter();
        _geigerRoutine = StartCoroutine(Co_RunGeigerEffect());
    }

    private void StopGeigerCounter()
    {
        if (_geigerRoutine != null)
        {
            StopCoroutine(_geigerRoutine);
            _geigerRoutine = null;
        }
    }

    private IEnumerator Co_RunGeigerEffect()
    {
        while (true)
        {
            if (_forceSolid || _currentIntensity >= 0.99f)
            {
                SetAlpha(1f);
            }
            else
            {
                float triggerThreshold = _baseNoiseLevel + (_currentIntensity * (1f - _baseNoiseLevel));

                if (Random.value < triggerThreshold)
                    SetAlpha(Random.Range(_minAlpha, _maxAlpha));
                else
                    SetAlpha(0f);
            }

            yield return new WaitForSeconds(_tickInterval);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (_warningImage == null)
            return;

        Color color = _warningImage.color;
        color.a = alpha;
        _warningImage.color = color;
    }
}