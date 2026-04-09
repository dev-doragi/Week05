using System;
using System.Collections.Generic;
using System.Linq; // 정렬 하려고 넣은 라이브러리
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    private static readonly int BackgroundPropertyId = Shader.PropertyToID("_background");

    [Header("Dependencies")]
    [SerializeField] private RawImage _background;
    [SerializeField] private CameraNoiseOverlay _cameraNoiseOverlay;
    [SerializeField] private CoachMovementController _coachMovementController;

    [Header("Debug View (Read Only)")]
    [SerializeField] private List<CameraAreaController> _registeredCameraList = new();

    private readonly Dictionary<RoomID, CameraAreaController> _registeredRooms = new();
    private Material _backgroundMaterial;
    private CameraAreaController _lastSelected;

    public event Action<RoomID> OnCameraSelected;

    private void Awake()
    {
        if (_coachMovementController == null)
            _coachMovementController = FindAnyObjectByType<CoachMovementController>();

        if (_cameraNoiseOverlay == null)
            _cameraNoiseOverlay = FindAnyObjectByType<CameraNoiseOverlay>();

        if (_background != null && _background.material != null)
        {
            _backgroundMaterial = Instantiate(_background.material);
            _background.material = _backgroundMaterial;
        }

        FindAndRegisterAllRooms();
    }

    private void OnEnable()
    {
        if (_coachMovementController != null)
        {
            _coachMovementController.OnCoachMoved += HandleCoachMoved;
            _coachMovementController.OnCoachPreparingToMove += HandleCoachPreparingToMove;
        }
    }

    private void OnDisable()
    {
        if (_coachMovementController != null)
        {
            _coachMovementController.OnCoachMoved -= HandleCoachMoved;
            _coachMovementController.OnCoachPreparingToMove -= HandleCoachPreparingToMove;
        }
    }

    private void Start()
    {
        if (_lastSelected != null)
            ApplyCameraTexture(_lastSelected);
    }

    private void FindAndRegisterAllRooms()
    {
        _registeredRooms.Clear();
        _registeredCameraList.Clear();

        var foundAreas = FindObjectsByType<CameraAreaController>(FindObjectsSortMode.None);

        // 이름 규칙(Cam숫자+접미사)에 따라 정렬 (예: Cam1 -> Cam1A -> Cam2)
        var sortedAreas = foundAreas.OrderBy(area => {
            var match = Regex.Match(area.gameObject.name, @"Cam(\d+)([A-Z]?)");
            if (match.Success)
            {
                int number = int.Parse(match.Groups[1].Value);
                string suffix = match.Groups[2].Value;
                return number * 100 + (suffix.Length > 0 ? suffix[0] : 0);
            }
            return int.MaxValue;
        }).ToList();

        foreach (var area in sortedAreas)
        {
            if (RegisterRoom(area))
            {
                _registeredCameraList.Add(area);
            }
        }
    }

    #region 정렬 없는 버전
    //private void FindAndRegisterAllRooms()
    //{
    //    _registeredRooms.Clear();
    //    _registeredCameraList.Clear();

    //    var foundAreas = UnityEngine.Object.FindObjectsByType<CameraAreaController>(FindObjectsSortMode.None);

    //    foreach (var area in foundAreas)
    //    {
    //        if (RegisterRoom(area))
    //        {
    //            _registeredCameraList.Add(area);
    //        }
    //    }
    //}
    #endregion

    public void SelectCamera(CameraAreaController selectedArea)
    {
        if (selectedArea == null) return;

        ApplyCameraTexture(selectedArea);

        if (_lastSelected != null && _lastSelected != selectedArea)
            _lastSelected.StopBlinking();

        selectedArea.StartBlinking();
        _lastSelected = selectedArea;

        if (_cameraNoiseOverlay != null)
            _cameraNoiseOverlay.PlaySwitchNoiseOnce();

        OnCameraSelected?.Invoke(selectedArea.RoomId);
    }

    public void RefreshSelectedCamera()
    {
        if (_lastSelected != null) ApplyCameraTexture(_lastSelected);
    }

    public bool RegisterRoom(CameraAreaController areaController)
    {
        if (areaController == null || areaController.RoomId == RoomID.None) return false;
        if (_registeredRooms.ContainsKey(areaController.RoomId)) return false;

        _registeredRooms.Add(areaController.RoomId, areaController);
        return true;
    }

    private void HandleCoachMoved(RoomID prev, RoomID curr) => RefreshSelectedCamera();

    private void HandleCoachPreparingToMove(RoomID prev, RoomID next)
    {
        if (IsViewingRoom(prev) || IsViewingRoom(next))
        {
            if (_cameraNoiseOverlay != null && _coachMovementController != null)
                _cameraNoiseOverlay.ShowForcedNoise(_coachMovementController.TransitionDuration);
        }
    }

    private void ApplyCameraTexture(CameraAreaController selectedArea)
    {
        if (selectedArea == null) return;

        bool hasCoach = _coachMovementController != null && _coachMovementController.IsCoachInRoom(selectedArea.RoomId);
        Texture targetTexture = selectedArea.GetBackgroundTexture(hasCoach);

        if (_backgroundMaterial != null)
            _backgroundMaterial.SetTexture(BackgroundPropertyId, targetTexture);
        else if (_background != null)
            _background.texture = targetTexture;
    }

    public bool IsViewingRoom(RoomID roomId) => _lastSelected != null && _lastSelected.RoomId == roomId;
}