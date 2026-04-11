using System;
using UnityEngine;
using UnityEngine.UI;

public class RGBObject : MonoBehaviour
{
    [SerializeField] private RGBPanel _panel;
    [SerializeField] private int _objectID;
    [SerializeField] private SpriteRenderer _targetRect;

    [SerializeField] private Slider _sliderR;
    [SerializeField] private Slider _sliderG;
    [SerializeField] private Slider _sliderB;

    [SerializeField] private MissionClearer _missionClearer;

    
    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _clearThreshold = 0.1f;
    // 목표 색상
    [SerializeField] private Color _targetColor;
    // 초기 색상
    [SerializeField] private Color _defaultColor;

    public event Action<int> Success;
    public bool IsCleared { get; private set; } = false;

    private bool _isInitialized;


    void Awake()
    {
        
    }

    public void Init(int id)
    {
        SetID(id);

        _sliderR.onValueChanged.AddListener(_ => UpdatePlayerColor());
        _sliderG.onValueChanged.AddListener(_ => UpdatePlayerColor());
        _sliderB.onValueChanged.AddListener(_ => UpdatePlayerColor());

        _sliderR.SetValueWithoutNotify(_defaultColor.r);
        _sliderG.SetValueWithoutNotify(_defaultColor.g);
        _sliderB.SetValueWithoutNotify(_defaultColor.b);


        _targetRect.color = _defaultColor;

        UpdatePlayerColor();
    }

    private void SetID(int id)
    {
        _objectID = id;
    }   

    void UpdatePlayerColor()
    {
        _targetRect.color = new Color(_sliderR.value, _sliderG.value, _sliderB.value);
        SuccessCheck();
    }

    void SuccessCheck()
    {
        float diff = ColorDifference(_targetRect.color, _targetColor);

        Debug.Log($"[RGBMatchGame] 오차: {diff:F3} / 기준: {_clearThreshold:F3}");

        if (diff <= _clearThreshold)
        {
            IsCleared = true;
            _panel.Panel.gameObject.SetActive(false);
            _targetRect.color = _targetColor;
            Success.Invoke(_objectID);
        }

    }

    float ColorDifference(Color a, Color b)
    {
        return (Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b)) / 3f;
    }
}
