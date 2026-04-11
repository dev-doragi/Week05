using System;
using UnityEngine;

public abstract class Stage : MonoBehaviour
{
    public static event Action<Stage> OnStageChanged;
    public static event Action OnClear;
    public static event Action<string> OnSendMessage;

    [SerializeField] private StageDefinition stageSO;
    public StageDefinition StageSO => stageSO;

    [SerializeField] private StageMission[] _runtimeMissions;
    public StageMission[] RuntimeMissions => _runtimeMissions;

    private bool _isCleared = false;

    protected virtual void Awake()
    {
        _runtimeMissions = (StageMission[])stageSO.Missions.Clone();
        Debug.Log("[Stage] Misstion Count: " + _runtimeMissions.Length);
    }

    public void StartStage()
    {
        Debug.Log(StageSO.StageId);
        gameObject.SetActive(true);
        OnSendMessage?.Invoke(StageSO.StartMessage);
        OnStart();
    }

    protected void Clear()
    {
        Debug.Log("Stage Clear!");
        _isCleared = true;
        OnStageChanged?.Invoke(this);
        gameObject.SetActive(false);
        OnClear?.Invoke();
    }

    protected abstract void OnStart();

    public void ClearMission(int index)
    {
        Debug.Log($"Clear Mission {index}");
        if (index < 0 || index >= _runtimeMissions.Length) return;
        if (_runtimeMissions[index].isMissionSuccess) return;

        _runtimeMissions[index].isMissionSuccess = true;

        if (!_isCleared && IsAllMissionCleared())
        {
            Clear();
            return;
        }

        OnStageChanged?.Invoke(this);
        OnSendMessage?.Invoke(_runtimeMissions[index].missionMessage);
    }

    private bool IsAllMissionCleared()
    {
        foreach (var m in _runtimeMissions)
            if (!m.isMissionSuccess) return false;
        return true;
    }

    public void InvokeChange()
    {
        Debug.Log("Invoke Stage Change");
        OnStageChanged?.Invoke(this);
    }
}