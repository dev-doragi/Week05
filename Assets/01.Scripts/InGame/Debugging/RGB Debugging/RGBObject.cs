using System;
using UnityEngine;
using UnityEngine.UI;

public class RGBObject : MonoBehaviour
{
    [SerializeField] private RGBPanel _panel;
   
    private int _objectID;
    private Transform _spawnPos;

    [SerializeField] private SpriteRenderer _targetRect;
    [SerializeField] private Collider2D _collider;

    [SerializeField] private Slider _sliderR;
    [SerializeField] private Slider _sliderG;
    [SerializeField] private Slider _sliderB;

    [SerializeField] private MissionClearer _missionClearer;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _clearThreshold = 0.1f;
    [SerializeField] private Color _targetColor;
    [SerializeField] private Color _defaultColor;

    public event Action<int> Success;
    public bool IsCleared { get; private set; } = false;

    private bool _isInitialized;

    private void Awake()
    {
        if (_missionClearer == null)
            _missionClearer = GetComponent<MissionClearer>();

        if (_missionClearer == null)
            _missionClearer = GetComponentInParent<MissionClearer>();
    }

    public void Init(int id, Transform spawnPos)
    {
        SetID(id);
        _spawnPos = spawnPos;
        IsCleared = false;

        if (_isInitialized == false)
        {
            _sliderR.onValueChanged.AddListener(HandleSliderChanged);
            _sliderG.onValueChanged.AddListener(HandleSliderChanged);
            _sliderB.onValueChanged.AddListener(HandleSliderChanged);
            _isInitialized = true;
        }

        _sliderR.SetValueWithoutNotify(_defaultColor.r);
        _sliderG.SetValueWithoutNotify(_defaultColor.g);
        _sliderB.SetValueWithoutNotify(_defaultColor.b);

        _targetRect.color = _defaultColor;
        _collider.isTrigger = true;
        UpdatePlayerColor();
    }

    private void SetID(int id)
    {
        _objectID = id;
    }

    private void HandleSliderChanged(float _)
    {
        UpdatePlayerColor();
    }

    private void UpdatePlayerColor()
    {
        _targetRect.color = new Color(_sliderR.value, _sliderG.value, _sliderB.value);
        SuccessCheck();
    }

    private void SuccessCheck()
    {
        if (IsCleared) return;

        float diff = ColorDifference(_targetRect.color, _targetColor);

        Debug.Log($"[RGBMatchGame] 오차: {diff:F3} / 기준: {_clearThreshold:F3}");

        if (diff <= _clearThreshold)
        {
            IsCleared = true;
            _collider.isTrigger = false;

            if (_panel != null && _panel.Panel != null)
                _panel.Panel.gameObject.SetActive(false);

            _targetRect.color = _targetColor;

            if (_missionClearer != null)
                _missionClearer.ClearMission();
            else
                Debug.LogWarning($"[RGBObject] MissionClearer is missing on {name}");

            Success?.Invoke(_objectID);
        }
    }

    private float ColorDifference(Color a, Color b)
    {
        return (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b)) / 3f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCleared) return;
        if (_spawnPos == null) return;
        if (!other.CompareTag("Player")) return;
            
        other.transform.position = _spawnPos.position;
    }

}
