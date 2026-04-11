using UnityEngine;
using System.Collections.Generic;

public class PassageCoachBlinkController : MonoBehaviour
{
    [SerializeField] private CoachMovementController coach;
    [SerializeField] private PassageTile[] passageTiles;
    [SerializeField] private bool autoFindPassages = true;

    

    private bool _coachFound;


    private void Awake()
    {
        if (coach == null)
            coach = FindAnyObjectByType<CoachMovementController>();

        if (autoFindPassages && (passageTiles == null || passageTiles.Length == 0))
            passageTiles = FindObjectsByType<PassageTile>(FindObjectsSortMode.None);
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
        if (coach != null)
        {
            coach.OnCoachMoved -= HandleCoachMoved;
            coach.OnCoachPreparingToMove -= HandleCoachPreparingToMove;
            CameraManager.Instance.OnCameraSelected -= HandleCameraSelected;

        }

        ClearAllBlink();
    }

    private void HandleCoachMoved(RoomID from, RoomID current)
    {
        if (_coachFound)
        {
            _coachFound = false;
            ClearAllBlink();
        }
    }

    private void HandleCoachPreparingToMove(RoomID from, RoomID to)
    {
        if (_coachFound)
        {
            _coachFound = false;
            ClearAllBlink();
        }
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

            tile.SetCoachBlink(shouldBlink);
        }
    }

    private void ClearAllBlink()
    {
        if (passageTiles == null) return;

        for (int i = 0; i < passageTiles.Length; i++)
        {
            PassageTile tile = passageTiles[i];
            if (tile == null) continue;
            tile.SetCoachBlink(false);
        }
    }
    private void HandleCameraSelected(RoomID selectedRoom)
    {
        if (coach == null)
        {
            _coachFound = false;
            ClearAllBlink();
            return;
        }

        // "코치가 있는 방을 찾았을 때만" true
        _coachFound = (selectedRoom != RoomID.None && selectedRoom == coach.CurrentRoomId);

        if (_coachFound) ApplyBlinkFromCurrentRoom(coach.CurrentRoomId);
        else ClearAllBlink();
    }

}
