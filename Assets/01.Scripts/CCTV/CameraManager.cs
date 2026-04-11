using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    private static readonly int BackgroundPropertyId = Shader.PropertyToID("_background");

    public static CameraManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private RawImage _background;
    [SerializeField] private CameraNoiseOverlay _cameraNoiseOverlay;
    [SerializeField] private CoachMovementController _coachMovementController;

    [Header("Debug View (Read Only)")]
    [SerializeField] private List<CameraAreaController> _registeredCameraList = new();

    private readonly Dictionary<RoomID, CameraAreaController> _registeredRooms = new();

    private Material _backgroundMaterial;
    private CameraAreaController _selectedArea;
    private bool _isHardMode;

    public event Action<RoomID> OnCameraSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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

        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (_selectedArea != null)
            ApplyCameraTexture(_selectedArea);

        RefreshAllBlinkStates();
    }

    private void Update()
    {
        RefreshAllBlinkStates();
    }

    private void FindAndRegisterAllRooms()
    {
        _registeredRooms.Clear();
        _registeredCameraList.Clear();

        var foundAreas = FindObjectsByType<CameraAreaController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var sortedAreas = foundAreas.OrderBy(area =>
        {
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
                _registeredCameraList.Add(area);
        }
    }

    public void SelectCamera(CameraAreaController selectedArea)
    {
        if (selectedArea == null)
            return;

        _selectedArea = selectedArea;

        ApplyCameraTexture(selectedArea);
        RefreshAllBlinkStates();

        if (_cameraNoiseOverlay != null)
            _cameraNoiseOverlay.PlaySwitchNoiseOnce();

        OnCameraSelected?.Invoke(selectedArea.RoomId);
    }

    public bool SelectCameraByRoomId(RoomID roomId)
    {
        if (roomId == RoomID.None)
            return false;

        if (_registeredRooms.TryGetValue(roomId, out CameraAreaController area) == false || area == null)
            return false;

        SelectCamera(area);
        return true;
    }


    public void RefreshSelectedCamera()
    {
        if (_selectedArea != null)
            ApplyCameraTexture(_selectedArea);
    }

    public bool RegisterRoom(CameraAreaController areaController)
    {
        if (areaController == null || areaController.RoomId == RoomID.None)
            return false;

        if (_registeredRooms.ContainsKey(areaController.RoomId))
            return false;

        _registeredRooms.Add(areaController.RoomId, areaController);
        return true;
    }

    public void SetHardMode(bool active)
    {
        _isHardMode = active;
        RefreshAllBlinkStates();
    }

    public bool IsViewingRoom(RoomID roomId)
    {
        return _selectedArea != null && _selectedArea.RoomId == roomId;
    }

    private void HandleCoachMoved(RoomID prevRoomId, RoomID currentRoomId)
    {
        RefreshSelectedCamera();
        RefreshAllBlinkStates();
    }

    private void HandleCoachPreparingToMove(RoomID prevRoomId, RoomID nextRoomId)
    {
        if (IsViewingRoom(prevRoomId) || IsViewingRoom(nextRoomId))
        {
            if (_cameraNoiseOverlay != null && _coachMovementController != null)
                _cameraNoiseOverlay.ShowForcedNoise(_coachMovementController.TransitionDuration);
        }

        RefreshAllBlinkStates();
    }

    private void RefreshAllBlinkStates()
    {
        for (int i = 0; i < _registeredCameraList.Count; i++)
        {
            CameraAreaController area = _registeredCameraList[i];

            if (area == null)
                continue;

            //area.SetBlinkState(GetBlinkState(area));
        }
    }

    // private CameraAreaController.BlinkState GetBlinkState(CameraAreaController area)
    // {
    //     if (area == null)
    //         return CameraAreaController.BlinkState.None;

    //     if (IsSelectedArea(area))
    //         return CameraAreaController.BlinkState.Selected;

    //     if (ShouldShowCoachDetection(area.RoomId))
    //         return CameraAreaController.BlinkState.CoachDetected;

    //     return CameraAreaController.BlinkState.None;
    // }

    // private bool IsSelectedArea(CameraAreaController area)
    // {
    //     return _selectedArea == area;
    // }

    // private bool ShouldShowCoachDetection(RoomID roomId)
    // {
    //     if (_isHardMode)
    //         return false;

    //     if (IsBuildIssueActive())
    //         return false;

    //     if (_coachMovementController == null)
    //         return false;

    //     return _coachMovementController.IsCoachInRoom(roomId);
    // }

    // private bool IsBuildIssueActive()
    // {
    //     return GameFlowManager.Instance != null &&
    //            GameFlowManager.Instance.State == FlowState.Debug;
    // }

    private void ApplyCameraTexture(CameraAreaController selectedArea)
    {
        if (selectedArea == null)
            return;

        bool hasCoach = _coachMovementController != null &&
                        _coachMovementController.IsCoachInRoom(selectedArea.RoomId);

        Texture targetTexture = selectedArea.GetBackgroundTexture(hasCoach);

        if (_backgroundMaterial != null)
            _backgroundMaterial.SetTexture(BackgroundPropertyId, targetTexture);
        else if (_background != null)
            _background.texture = targetTexture;
    }
}