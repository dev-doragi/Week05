using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    private static readonly int BackgroundPropertyId = Shader.PropertyToID("_background");

    [SerializeField] private RawImage _background;
    [SerializeField] private CameraNoiseOverlay _cameraNoiseOverlay;
    [SerializeField] private CoachMovementController _coachMovementController;
    [SerializeField] private CameraAreaController[] _cameraAreas;

    private readonly Dictionary<RoomID, CameraAreaController> _registeredRooms = new();

    private Material _backgroundMaterial;
    private CameraAreaController _lastSelected;

    private void Awake()
    {
        if (_background != null && _background.material != null)
        {
            _backgroundMaterial = Instantiate(_background.material);
            _background.material = _backgroundMaterial;
        }

        RegisterAllRooms();
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

    public void SelectCamera(CameraAreaController selectedArea)
    {
        if (selectedArea == null)
            return;

        ApplyCameraTexture(selectedArea);

        if (_lastSelected != null && _lastSelected != selectedArea)
            _lastSelected.StopBlinking();

        selectedArea.StartBlinking();
        _lastSelected = selectedArea;

        if (_cameraNoiseOverlay != null)
            _cameraNoiseOverlay.PlaySwitchNoiseOnce();
    }

    public void RefreshSelectedCamera()
    {
        if (_lastSelected == null)
            return;

        ApplyCameraTexture(_lastSelected);
    }

    public bool RegisterRoom(CameraAreaController areaController)
    {
        if (areaController == null)
            return false;

        if (areaController.RoomId == RoomID.None)
            return false;

        if (_registeredRooms.ContainsKey(areaController.RoomId))
            return false;

        _registeredRooms.Add(areaController.RoomId, areaController);
        return true;
    }

    public CameraAreaController GetRoom(RoomID roomId)
    {
        if (_registeredRooms.TryGetValue(roomId, out CameraAreaController areaController))
            return areaController;

        return null;
    }

    private void RegisterAllRooms()
    {
        _registeredRooms.Clear();

        if (_cameraAreas == null)
            return;

        for (int i = 0; i < _cameraAreas.Length; i++)
        {
            RegisterRoom(_cameraAreas[i]);
        }
    }

    private void HandleCoachMoved(RoomID previousRoomId, RoomID currentRoomId)
    {
        RefreshSelectedCamera();
    }

    private void HandleCoachPreparingToMove(RoomID previousRoomId, RoomID nextRoomId)
    {
        if (!IsViewingRoom(previousRoomId))
            return;

        if (_cameraNoiseOverlay == null || _coachMovementController == null)
            return;

        _cameraNoiseOverlay.ShowForcedNoise(_coachMovementController.TransitionDuration);
    }

    private void ApplyCameraTexture(CameraAreaController selectedArea)
    {
        if (selectedArea == null)
            return;

        bool hasCoach = _coachMovementController != null &&
                        _coachMovementController.IsCoachInRoom(selectedArea.RoomId);

        Texture targetTexture = selectedArea.GetBackgroundTexture(hasCoach);

        if (_backgroundMaterial != null)
        {
            _backgroundMaterial.SetTexture(BackgroundPropertyId, targetTexture);
            return;
        }

        if (_background != null)
            _background.texture = targetTexture;
    }

    public bool IsViewingRoom(RoomID roomId)
    {
        return _lastSelected != null && _lastSelected.RoomId == roomId;
    }
}