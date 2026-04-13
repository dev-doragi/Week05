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
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private Button _interactionButton;
    [SerializeField] private TMP_Text _buttonLabel;
    [SerializeField] private RectTransform _cooldownGaugeRect;

    [Header("References")]
    [SerializeField] private CoachMovementController _coachMovementController;

    [Header("Settings")]
    [SerializeField] private List<RoomGimmickSetting> _settings = new();
    [SerializeField] private float _maxWidth = 1000f;

    private CameraManager _cameraManager;
    private GimmickManager _gimmickManager;

    private RoomGimmickSetting? _currentActiveSetting;
    private readonly Dictionary<RoomID, float> _cooldownEndTimeMap = new();

    private void Awake()
    {
        _cameraManager = FindAnyObjectByType<CameraManager>();

        if (_coachMovementController == null)
            _coachMovementController = FindAnyObjectByType<CoachMovementController>();
    }

    private void OnEnable()
    {
        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        if (_cameraManager != null)
            _cameraManager.OnCameraSelected += UpdateUIByRoom;

        if (_interactionButton != null)
            _interactionButton.onClick.AddListener(HandleButtonClick);

        RefreshPanelVisibility();
        RefreshButtonInteractable();
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
        RefreshPanelVisibility();
        RefreshButtonInteractable();
    }

    private void UpdateUIByRoom(RoomID currentRoom)
    {
        RoomGimmickSetting setting = _settings.Find(s => s.Room == currentRoom);

        if (setting.GimmickType != GimmickType.None)
        {
            _currentActiveSetting = setting;

            if (_buttonLabel != null)
                _buttonLabel.text = setting.ButtonText;
        }
        else
        {
            _currentActiveSetting = null;

            if (_buttonLabel != null)
                _buttonLabel.text = string.Empty;

            SetGaugeWidth(0f);
        }

        RefreshPanelVisibility();
        RefreshButtonInteractable();
    }

    private void HandleButtonClick()
    {
        if (_currentActiveSetting == null)
            return;

        if (IsPanelVisible() == false)
            return;

        RoomGimmickSetting data = _currentActiveSetting.Value;

        if (IsOnCooldown(data.Room))
            return;

        if (_gimmickManager == null || _gimmickManager.CanUseGimmick == false)
            return;

        _cooldownEndTimeMap[data.Room] = Time.time + data.Cooldown;

        switch (data.GimmickType)
        {
            case GimmickType.RequestInterview:
                _gimmickManager.SetTempTarget(data.Room, data.Duration);
                break;

            case GimmickType.DisableF3Button:
            case GimmickType.PromoteSpringMenu:
                _gimmickManager.ActivateStayDelay(data.GimmickType, data.Room, data.Value, data.Duration);
                break;
        }

        RefreshPanelVisibility();
        RefreshButtonInteractable();
    }

    private void UpdateCooldownUI()
    {
        if (_currentActiveSetting == null)
        {
            SetGaugeWidth(0f);
            return;
        }

        RoomID currentRoom = _currentActiveSetting.Value.Room;

        if (_cooldownEndTimeMap.TryGetValue(currentRoom, out float endTime))
        {
            float remaining = endTime - Time.time;

            if (remaining > 0f)
            {
                float progress = remaining / _currentActiveSetting.Value.Cooldown;
                SetGaugeWidth(progress * _maxWidth);
            }
            else
            {
                SetGaugeWidth(0f);
            }
        }
        else
        {
            SetGaugeWidth(0f);
        }
    }

    private void SetGaugeWidth(float width)
    {
        if (_cooldownGaugeRect == null)
            return;

        _cooldownGaugeRect.sizeDelta = new Vector2(width, _cooldownGaugeRect.sizeDelta.y);
    }

    private void RefreshPanelVisibility()
    {
        if (_panelRoot == null)
            return;

        _panelRoot.SetActive(IsPanelVisible());
    }

    private bool IsPanelVisible()
    {
        if (_currentActiveSetting == null)
            return false;

        if (_coachMovementController != null && _coachMovementController.IsTransitioning)
            return false;

        return true;
    }

    private void RefreshButtonInteractable()
    {
        if (_interactionButton == null)
            return;

        if (IsPanelVisible() == false)
        {
            _interactionButton.interactable = false;
            return;
        }

        if (_gimmickManager == null)
        {
            _interactionButton.interactable = false;
            return;
        }

        bool isOnCooldown = IsOnCooldown(_currentActiveSetting.Value.Room);
        bool canUseNow = _gimmickManager.CanUseGimmick;

        _interactionButton.interactable = !isOnCooldown && canUseNow;
    }

    private bool IsOnCooldown(RoomID room)
    {
        if (_cooldownEndTimeMap.TryGetValue(room, out float endTime))
            return Time.time < endTime;

        return false;
    }
}