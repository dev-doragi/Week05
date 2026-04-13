using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GimmickType
{
    None,
    RequestInterview,
    DisableF3Button,
    PromoteSpringMenu
}

[Serializable]
public struct RoomGimmickSetting
{
    public RoomID Room;
    public GimmickType GimmickType;
    public string ButtonText;
    public float Duration;
    public float Value;
    public float Cooldown;
}

public class GimmickController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _interactionButton;
    [SerializeField] private TMP_Text _buttonLabel;
    [SerializeField] private RectTransform _cooldownGaugeRect;

    [Header("Settings")]
    [SerializeField] private List<RoomGimmickSetting> _settings = new();
    [SerializeField] private float _maxWidth = 1000f;

    // 인스펙터 할당 대신 런타임 동적 할당 사용
    private CameraManager _cameraManager;
    private GimmickManager _gimmickManager;

    private RoomGimmickSetting? _currentActiveSetting;
    private Dictionary<RoomID, float> _cooldownEndTimeMap = new();

    private void Awake()
    {
        _cameraManager = FindAnyObjectByType<CameraManager>();
        _gimmickManager = FindAnyObjectByType<GimmickManager>();
    }

    private void OnEnable()
    {
        if (_cameraManager != null)
            _cameraManager.OnCameraSelected += UpdateUIByRoom;

        if (_interactionButton != null)
            _interactionButton.onClick.AddListener(HandleButtonClick);
    }

    private void OnDisable()
    {
        if (_cameraManager != null)
            _cameraManager.OnCameraSelected -= UpdateUIByRoom;

        if (_interactionButton != null)
            _interactionButton.onClick.RemoveListener(HandleButtonClick);
    }

    private void Update()
    {
        UpdateCooldownUI();
    }

    private void UpdateUIByRoom(RoomID currentRoom)
    {
        var setting = _settings.Find(s => s.Room == currentRoom);

        if (setting.GimmickType != GimmickType.None)
        {
            _currentActiveSetting = setting;
            _buttonLabel.text = setting.ButtonText;
            RefreshButtonInteractable();
        }
        else
        {
            _currentActiveSetting = null;
            _buttonLabel.text = "";
            _interactionButton.interactable = false;
            SetGaugeWidth(0f);
        }
    }

    private void HandleButtonClick()
    {
        if (_currentActiveSetting == null || IsOnCooldown(_currentActiveSetting.Value.Room))
            return;

        RoomGimmickSetting data = _currentActiveSetting.Value;
        _cooldownEndTimeMap[data.Room] = Time.time + data.Cooldown;

        switch (data.GimmickType)
        {
            case GimmickType.RequestInterview:
                if (_gimmickManager != null)
                    _gimmickManager.SetTempTarget(data.Room, data.Duration);
                break;

            case GimmickType.DisableF3Button:
            case GimmickType.PromoteSpringMenu:
                if (_gimmickManager != null)
                    _gimmickManager.ActivateStayDelay(data.GimmickType, data.Room, data.Value, data.Duration);
                break;
        }
    }

    private void UpdateCooldownUI()
    {
        if (_currentActiveSetting == null) return;

        RoomID currentRoom = _currentActiveSetting.Value.Room;

        if (_cooldownEndTimeMap.TryGetValue(currentRoom, out float endTime))
        {
            float remaining = endTime - Time.time;
            if (remaining > 0)
            {
                float progress = remaining / _currentActiveSetting.Value.Cooldown;
                SetGaugeWidth(progress * _maxWidth);

                if (_interactionButton.interactable)
                    _interactionButton.interactable = false;
            }
            else
            {
                SetGaugeWidth(0f);
                if (!_interactionButton.interactable)
                    _interactionButton.interactable = true;
            }
        }
        else
        {
            SetGaugeWidth(0f);
        }
    }

    private void SetGaugeWidth(float width)
    {
        if (_cooldownGaugeRect == null) return;
        _cooldownGaugeRect.sizeDelta = new Vector2(width, _cooldownGaugeRect.sizeDelta.y);
    }

    private void RefreshButtonInteractable()
    {
        if (_currentActiveSetting == null) return;
        _interactionButton.interactable = !IsOnCooldown(_currentActiveSetting.Value.Room);
    }

    private bool IsOnCooldown(RoomID room)
    {
        if (_cooldownEndTimeMap.TryGetValue(room, out float endTime))
        {
            return Time.time < endTime;
        }
        return false;
    }
}