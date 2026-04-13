using UnityEngine;
using System.Collections.Generic;


public class PassageCoachBlinkController : MonoBehaviour
{
    [SerializeField] private CoachMovementController coach;
    [SerializeField] private PassageTile[] passageTiles;
    [SerializeField] private bool autoFindPassages = true;
    

    

    private bool _coachFound;
    private RoomID _selectedRoom = RoomID.None;

    private void Awake()
    {
        if (coach == null)
            coach = FindAnyObjectByType<CoachMovementController>();

        if (autoFindPassages && (passageTiles == null || passageTiles.Length == 0))
            passageTiles = FindObjectsByType<PassageTile>(FindObjectsSortMode.None);

        BindPassageEvents(true);

    }

    private void Start()
    {
        if (coach == null) return;

        coach.OnCoachMoved += HandleCoachMoved;
        coach.OnCoachPreparingToMove += HandleCoachPreparingToMove;
        CameraManager.Instance.OnCameraSelected += HandleCameraSelected;

    }

    private void OnDisable()
    {
        BindPassageEvents(false);

        if (coach != null)
        {
            coach.OnCoachMoved -= HandleCoachMoved;
            coach.OnCoachPreparingToMove -= HandleCoachPreparingToMove;
            if(CameraManager.Instance != null)
            {
                CameraManager.Instance.OnCameraSelected -= HandleCameraSelected;
            }

        }

        ClearAllBlink();
    }

    private void HandleCoachMoved(RoomID from, RoomID current)
    {
        RefreshGuideByState(); 
    }

    private void HandleCoachPreparingToMove(RoomID from, RoomID to)
    {
        _coachFound = false;
        ClearAllBlink();
    }

    private void HandleCameraSelected(RoomID selectedRoom)
    {
        _selectedRoom = selectedRoom;
        RefreshGuideByState();
    }



    private void ApplyBlinkFromCurrentRoom(RoomID currentRoom)
    {
        ClearAllBlink();

        if (coach == null || coach.MapGraph == null) return;
        if (currentRoom == RoomID.None || currentRoom == RoomID.Office) return;

        IReadOnlyList<RoomID> neighbors = coach.MapGraph.GetNeighbors(currentRoom);

        for (int i = 0; i < passageTiles.Length; i++)
        {
            PassageTile tile = passageTiles[i];
            if (tile == null) continue;

            bool shouldBlink = false;
            for (int j = 0; j < neighbors.Count; j++)
            {
                if (tile.Connects(currentRoom, neighbors[j]))
                {
                    shouldBlink = true;
                    break;
                }
            }

            if (shouldBlink) tile.ShowGuideFrom(currentRoom);
            else tile.HideGuide();
        }
    }

    private void ClearAllBlink()
    {
        if (passageTiles == null) return;

        for (int i = 0; i < passageTiles.Length; i++)
        {
            PassageTile tile = passageTiles[i];
            if (tile == null) continue;
            tile.HideGuide();

        }
    }
    
    private void RefreshGuideByState()
    {
        if (coach == null)
        {
            _coachFound = false;
            ClearAllBlink();
            return;
        }

        bool canShow =
            _selectedRoom != RoomID.None &&
            !coach.IsTransitioning &&
            coach.CurrentRoomId == _selectedRoom;

        _coachFound = canShow;

        if (_coachFound) ApplyBlinkFromCurrentRoom(coach.CurrentRoomId);
        else ClearAllBlink();
    }

    private void BindPassageEvents(bool bind)
    {
        if (passageTiles == null) return;

        for (int i = 0; i < passageTiles.Length; i++)
        {
            PassageTile tile = passageTiles[i];
            if (tile == null) continue;

            if (bind) tile.BlockVisualChanged += HandlePassageBlockVisualChanged;
            else tile.BlockVisualChanged -= HandlePassageBlockVisualChanged;
        }
    }

    private void HandlePassageBlockVisualChanged(PassageTile tile, bool isBlocked)
    {
        RefreshGuideByState();
    }



}
