using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_WarningIndicator : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coach;
    [SerializeField] private Image _warningImage;

    [Header("Geiger Settings")]
    [Range(0f, 1f)][SerializeField] private float _baseNoiseLevel = 0.05f; // 평상시 지지직거리는 기본 확률
    [SerializeField] private float _maxDistance = 5f; // 감지 시작 최대 거리
    [SerializeField] private float _minAlpha = 0.1f;
    [SerializeField] private float _maxAlpha = 0.8f;

    [Header("Timing")]
    [SerializeField] private float _tickInterval = 0.02f; // 확률 체크 주기 (초)

    private Coroutine _geigerRoutine;
    private float _currentIntensity = 0f; // 0 (멂) ~ 1 (사무실 도달)
    private bool _forceSolid;

    private void OnEnable()
    {
        if (_coach != null)
        {
            _coach.OnCoachPreparingToMove += HandleCoachPreparingToMove;
            _coach.OnCoachMoved += HandleCoachMoved;
        }
        StartGeigerCounter();
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

        int distance = _coach.MapGraph.GetDistance(currentRoom, RoomID.Office);

        // 거리에 따른 강도 계산 (역수 관계)
        // distance 0(사무실 도달) -> Intensity 1
        // distance 가 멀어질수록 0에 수렴
        if (distance <= 0) _currentIntensity = 1f;
        else
        {
            _currentIntensity = Mathf.Clamp01(1f - (distance / _maxDistance));
        }
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
            if (_forceSolid || (_currentIntensity >= 0.99f))
            {
                SetAlpha(1f);
            }
            else
            {
                // 가이거 계수기 원리: 기본 소음 + 거리에 따른 입자 검출 확률 증가
                float triggerThreshold = _baseNoiseLevel + (_currentIntensity * (1f - _baseNoiseLevel));

                if (Random.value < triggerThreshold)
                {
                    // 불규칙한 강도로 번쩍임
                    SetAlpha(Random.Range(_minAlpha, _maxAlpha));
                }
                else
                {
                    // 신호 없음
                    SetAlpha(0f);
                }
            }

            yield return new WaitForSeconds(_tickInterval);
        }
    }

    private void SetAlpha(float alpha)
    {
        if (_warningImage == null) return;
        Color color = _warningImage.color;
        color.a = alpha;
        _warningImage.color = color;
    }
}