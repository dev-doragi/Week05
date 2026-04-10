// StageController.cs
using UnityEngine;
using System;

public abstract class StageController : MonoBehaviour
{
    [SerializeField] private StageDefinition stageDef;

    public static event Action<StageController> OnStageChanged;

    public StageDefinition StageDef => stageDef;
    public StageMission[] Missions { get; private set; }
    public bool IsStageClear { get; private set; }

    void Start()
    {
    }


    public void StartStage()
    {
        Missions = new StageMission[stageDef.Missions.Length];
        for (int i = 0; i < Missions.Length; i++)
        {
            Missions[i] = new StageMission
            {
                missionTitle = stageDef.Missions[i].missionTitle,
                isMissionSuccess = false
            };
        }

        gameObject.SetActive(true);
        OnStart();
    }

    protected abstract void OnStart();


    public void ClearMission(int index)
    {
        Debug.Log($"Clear Mission {index}");
        if (index < 0 || index >= Missions.Length) return;
        if (Missions[index].isMissionSuccess) return;

        Missions[index].isMissionSuccess = true;

        if (!IsStageClear && IsAllMissionCleared())
        {
            Debug.Log("Stage Clear!");
            IsStageClear = true;
        }


        OnStageChanged?.Invoke(this);
    }

    private bool IsAllMissionCleared()
    {
        foreach (var m in Missions)
            if (!m.isMissionSuccess) return false;
        return true;
    }
}