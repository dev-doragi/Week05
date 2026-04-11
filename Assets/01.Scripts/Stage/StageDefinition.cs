// StageDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Stage", menuName = "DelayTheInevitable/Stage Definition")]
public class StageDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string stageId = "STAGE_001";
    [SerializeField] private string stageTitle = "플레이어 이동 불가";
    [SerializeField] private string startMessage = "플레이어가 움직이지가 않네 어떻게 해야할까";

    public string StageId => stageId;
    public string StageTitle => stageTitle;

    public string StartMessage => startMessage;


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