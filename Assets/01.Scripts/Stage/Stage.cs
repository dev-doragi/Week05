using System;
using UnityEngine;

public abstract class Stage : MonoBehaviour
{
    public static event Action<Stage> OnStageChanged;

    [SerializeField] private StageDefinition stageSO;
    public StageDefinition StageSO => stageSO;

    private StageMission[] _runtimeMissions;
    public StageMission[] RuntimeMissions => _runtimeMissions;

    protected virtual void Awake()
    {
        _runtimeMissions = (StageMission[])stageSO.Missions.Clone();
        Debug.Log("[Stage] Misstion Count: " + _runtimeMissions.Length);
    }

    public void StartGame()
    {
        Debug.Log(StageSO.StageId);
        gameObject.SetActive(true);
        OnStart();
    }

    protected void Clear()
    {
        OnStageChanged?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected abstract void OnStart();

    public void ClearMission(int index)
    {
        Debug.Log($"Clear Mission {index}");
        if (index < 0 || index >= _runtimeMissions.Length) return;
        if (_runtimeMissions[index].isMissionSuccess) return;

        _runtimeMissions[index].isMissionSuccess = true;

        if (IsAllMissionCleared())
        {
            Debug.Log("Stage Clear!");
            Clear();
            return;
        }

        OnStageChanged?.Invoke(this);
    }

    private bool IsAllMissionCleared()
    {
        foreach (var m in _runtimeMissions)
            if (!m.isMissionSuccess) return false;
        return true;
    }
}