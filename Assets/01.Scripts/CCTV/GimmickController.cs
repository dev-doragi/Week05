using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GimmickType { None, SoundLure, BlockElevator, RequestInterview }

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
    [Header("Dependencies")]
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private GimmickManager _gimmickManager;

    [Header("UI Components")]
    [SerializeField] private Button _interactionButton;
    [SerializeField] private TMP_Text _buttonLabel;
    [SerializeField] private RectTransform _cooldownGaugeRect;

    [Header("Settings")]
    [SerializeField] private List<RoomGimmickSetting> _settings = new();
    [SerializeField] private float _maxWidth = 1000f;

    private RoomGimmickSetting? _currentActiveSetting;
    private Dictionary<RoomID, float> _cooldownEndTimeMap = new();

    private void OnEnable()
    {
        if (_cameraManager != null)
            _cameraManager.OnCameraSelected += UpdateUIByRoom;

        _interactionButton.onClick.AddListener(HandleButtonClick);
    }

    private void OnDisable()
    {
        if (_cameraManager != null)
            _cameraManager.OnCameraSelected -= UpdateUIByRoom;

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
        if (_currentActiveSetting == null || IsOnCooldown(_currentActiveSetting.Value.Room)) return;

        var data = _currentActiveSetting.Value;
        _cooldownEndTimeMap[data.Room] = Time.time + data.Cooldown;

        switch (data.GimmickType)
        {
            case GimmickType.SoundLure:
                _gimmickManager.ActivateLure(data.Room, data.Value, data.Duration);
                break;
            case GimmickType.BlockElevator:
                if (data.Room == RoomID.Elevator_A)
                {
                    _gimmickManager.BlockPath(RoomID.Elevator_B, RoomID.Elevator_A, data.Duration);
                    _gimmickManager.DelayElevator();
                }
                break;
            case GimmickType.RequestInterview:
                _gimmickManager.SetTempTarget(RoomID.CoachingRoom, data.Duration);
                break;
        }

        RefreshButtonInteractable();
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