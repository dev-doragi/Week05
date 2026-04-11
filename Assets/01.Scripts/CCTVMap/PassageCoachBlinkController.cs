using UnityEngine;
using System.Collections.Generic;

public class PassageCoachBlinkController : MonoBehaviour
{
    [SerializeField] private CoachMovementController coach;
    [SerializeField] private PassageTile[] passageTiles;
    [SerializeField] private bool autoFindPassages = true;

    private void Awake()
    {
        if (coach == null)
            coach = FindAnyObjectByType<CoachMovementController>();

        if (autoFindPassages && (passageTiles == null || passageTiles.Length == 0))
            passageTiles = FindObjectsByType<PassageTile>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        if (coach == null) return;

        coach.OnCoachMoved += HandleCoachMoved;
        coach.OnCoachPreparingToMove += HandleCoachPreparingToMove;
    }

    private void OnDisable()
    {
        if (coach != null)
        {
            coach.OnCoachMoved -= HandleCoachMoved;
            coach.OnCoachPreparingToMove -= HandleCoachPreparingToMove;
        }

        ClearAllBlink();
    }

    private void HandleCoachMoved(RoomID from, RoomID current)
    {
        ApplyBlinkFromCurrentRoom(current);
    }

    private void HandleCoachPreparingToMove(RoomID from, RoomID to)
    {
        // "그 방에 있는 동안만" Blink 하려면 여기서 끔
        ClearAllBlink();
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
}
