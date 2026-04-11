// StageDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "DelayTheInevitable/Stage Definition")]
public class StageDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string stageId = "STAGE_001";
    [SerializeField] private string stageTitle = "플레이어 이동 불가";
    public string StageId => stageId;
    public string StageTitle => stageTitle;

    [Header("Missions")]
    [SerializeField] private StageMission[] missions;
    public StageMission[] Missions => missions;
}

[System.Serializable]
public struct StageMission
{
    public int missionIndex;
    public string missionTitle;
    public bool isMissionSuccess;
    public string missionMessage;
}