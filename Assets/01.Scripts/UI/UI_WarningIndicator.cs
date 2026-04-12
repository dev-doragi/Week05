using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_WarningIndicator : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coachMovementController;
    [SerializeField] private Image _warningImage;

    [Header("Intensity Multipliers by Floor")]
    [SerializeField] private float _b1Multiplier = 0.1f;
    [SerializeField] private float _f1Multiplier = 0.4f;
    [SerializeField] private float _f3Multiplier = 1.0f;

    [Header("Geiger Settings")]
    [Range(0f, 1f)][SerializeField] private float _baseNoiseLevel = 0.05f;
    [SerializeField] private float _maxDistance = 5f;
    [SerializeField] private float _minAlpha = 0.1f;
    [SerializeField] private float _maxAlpha = 0.8f;

    [Header("Timing")]
    [SerializeField] private float _tickInterval = 0.02f;

    private Coroutine _geigerRoutine;
    private float _currentIntensity = 0f;
    private bool _forceSolid;

    private void Awake()
    {
        if (_coachMovementController == null)
            _coachMovementController = FindFirstObjectByType<CoachMovementController>();
    }

    private void OnEnable()
    {
        if (_coachMovementController == null || _warningImage == null)
        {
            Debug.LogError("UI_WarningIndicator 참조 누락");
            return;
        }

        _coachMovementController.OnCoachPreparingToMove += HandleCoachPreparingToMove;
        _coachMovementController.OnCoachMoved += HandleCoachMoved;

        StartGeigerCounter();
        UpdateIntensity(_coachMovementController.CurrentRoomId);
    }

    private void OnDisable()
    {
        if (_coachMovementController != null)
        {
            _coachMovementController.OnCoachPreparingToMove -= HandleCoachPreparingToMove;
            _coachMovementController.OnCoachMoved -= HandleCoachMoved;
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
        UpdateIntensity(_coachMovementController.CurrentRoomId);
    }

    private void UpdateIntensity(RoomID currentRoom)
    {
        if (_coachMovementController == null || _coachMovementController.MapGraph == null || currentRoom == RoomID.None)
        {
            _currentIntensity = 0f;
            return;
        }

        int distance = _coachMovementController.MapGraph.GetDistance(currentRoom, RoomID.Office);
        float distanceFactor = Mathf.Clamp01(1f - ((float)distance / _maxDistance));

        float floorMultiplier = GetFloorMultiplier(currentRoom);
        float aggroFactor = _coachMovementController.CurrentAggro;

        _currentIntensity = Mathf.Clamp01(distanceFactor * floorMultiplier * aggroFactor);
    }

    private float GetFloorMultiplier(RoomID room)
    {
        string roomName = room.ToString();

        if (roomName.StartsWith("B1F")) return _b1Multiplier;
        if (roomName.StartsWith("F1")) return _f1Multiplier;
        if (roomName.StartsWith("F3")) return _f3Multiplier;
        if (room == RoomID.Office) return 1.0f;

        return 0f;
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
            if (_forceSolid || _currentIntensity >= 0.95f)
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
        {
            Debug.LogError("_warningImage가 null입니다.");
            return;
        }

        Color color = _warningImage.color;
        color.a = alpha;
        _warningImage.color = color;
    }
}