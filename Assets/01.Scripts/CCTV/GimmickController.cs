using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 기믹 데이터 정의 (인스펙터에서 설정)
[Serializable]
public struct RoomGimmickSetting
{
    public RoomID Room;
    public GimmickType GimmickType;
    public string ButtonText;
    public float Duration;
    public float Value;
}

public enum GimmickType { None, SoundLure, BlockElevator, RequestInterview }

public class GimmickController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CameraManager _cameraManager;
    [SerializeField] private GimmickManager _gimmickManager;

    [Header("UI Components")]
    [SerializeField] private Button _interactionButton;
    [SerializeField] private TMP_Text _buttonLabel;

    [Header("Settings")]
    [SerializeField] private List<RoomGimmickSetting> _settings = new();

    private RoomGimmickSetting? _currentActiveSetting;

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

    private void UpdateUIByRoom(RoomID currentRoom)
    {
        // 현재 방에 해당하는 기믹 설정 찾기
        var setting = _settings.Find(s => s.Room == currentRoom);

        if (setting.GimmickType != GimmickType.None)
        {
            _currentActiveSetting = setting;
            _interactionButton.interactable = true;
            _buttonLabel.text = setting.ButtonText;
        }
        else
        {
            _currentActiveSetting = null;
            _interactionButton.interactable = false;
            _buttonLabel.text = "Unavailable";
        }
    }

    private void HandleButtonClick()
    {
        if (_currentActiveSetting == null) return;

        var data = _currentActiveSetting.Value;

        switch (data.GimmickType)
        {
            case GimmickType.SoundLure:
                _gimmickManager.ActivateLure(data.Room, data.Value, data.Duration);
                break;
            case GimmickType.BlockElevator:
                // 엘리베이터 이동 경로 차단 (예: Elevator_A -> Stair_A)
                RoomID target = (data.Room == RoomID.Elevator_A) ? RoomID.Stair_A : RoomID.Stair_B;
                _gimmickManager.BlockPath(data.Room, target, data.Duration);
                break;
            case GimmickType.RequestInterview:
                _gimmickManager.SetTempTarget(RoomID.CoachingRoom, data.Duration);
                break;
        }
    }
}