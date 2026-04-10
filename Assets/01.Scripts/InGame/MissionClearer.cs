// MissionClearer.cs
using UnityEngine;

public class MissionClearer : MonoBehaviour
{
    [SerializeField] private Stage stage;
    [SerializeField] private int missionIndexToClear;

    public void ClearMission() => stage.ClearMission(missionIndexToClear);
}