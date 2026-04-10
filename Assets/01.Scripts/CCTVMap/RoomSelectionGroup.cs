using UnityEngine;

public class RoomSelectionGroup : MonoBehaviour
{
    [SerializeField] private RoomTile[] rooms;

    private void Start()
    {
        if (rooms == null || rooms.Length == 0)
            rooms = GetComponentsInChildren<RoomTile>(true);

        RoomTile firstSelected = null;
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == null) continue;
            if (rooms[i].IsSelected && firstSelected == null)
                firstSelected = rooms[i];
        }

        if (firstSelected != null) SelectRoom(firstSelected);
        else ClearSelection();
    }

    public void SelectRoom(RoomTile target)
    {
        if (target == null) return;

        for (int i = 0; i < rooms.Length; i++)
        {
            RoomTile room = rooms[i];
            if (room == null) continue;
            room.SetSelectedVisual(room == target);
        }

    }

    public void ClearSelection()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == null) continue;
            rooms[i].SetSelectedVisual(false);
        }
    }
}
