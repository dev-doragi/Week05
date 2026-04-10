using System.Collections.Generic;
using UnityEngine;

public class UI_ConsolePresenter : MonoBehaviour
{
    [Header("Initial Data")]
    [SerializeField] private List<StageDefinition> _stageDefinitions = new();

    [Header("View")]
    [SerializeField] private UI_ConsoleComponentView _viewPrefab;
    [SerializeField] private Transform _viewParent;

    private readonly Dictionary<string, UI_ConsoleComponentView> _views = new();

    private void Awake()
    {
        CreateAllViewsFromDefinitions();
        ApplyCurrentRuntimeStates();
    }

    private void OnEnable()
    {
        Stage.OnStageChanged += HandleStageChanged;
    }

    private void OnDisable()
    {
        Stage.OnStageChanged -= HandleStageChanged;
    }

    // SO 데이터를 기반으로 만든 UI 생성
    private void CreateAllViewsFromDefinitions()
    {
        if (_viewPrefab == null)
        {
            Debug.LogWarning("[UI_ConsolePresenter] View Prefab is missing.");
            return;
        }

        ClearAllViews();

        for (int i = 0; i < _stageDefinitions.Count; i++)
        {
            CreateView(_stageDefinitions[i]);
        }
    }

    private void CreateView(StageDefinition definition)
    {
        if (definition == null)
            return;

        if (string.IsNullOrEmpty(definition.StageId))
            return;

        if (_views.ContainsKey(definition.StageId))
            return;

        Transform parent = _viewParent != null ? _viewParent : transform;
        UI_ConsoleComponentView view = Instantiate(_viewPrefab, parent);

        _views.Add(definition.StageId, view);

        List<(int missionIndex, string missionTitle)> missionData = BuildMissionData(definition);

        view.Init(
            definition.StageId,
            definition.StageTitle,
            missionData,
            missionData.Count);
    }

    // 현재 씬에 있는 Stage들의 런타임 성공 상태를 한 번 반영
    private void ApplyCurrentRuntimeStates()
    {
        Stage[] stages = FindObjectsByType<Stage>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < stages.Length; i++)
        {
            ApplyRuntimeState(stages[i]);
        }
    }

    private void HandleStageChanged(Stage changedStage)
    {
        if (changedStage == null)
            return;

        ApplyRuntimeState(changedStage);
    }

    // 런타임 데이터로 성공 여부 체크
    private void ApplyRuntimeState(Stage stage)
    {
        if (stage == null || stage.StageSO == null)
            return;

        string stageId = stage.StageSO.StageId;
        if (string.IsNullOrEmpty(stageId))
            return;

        if (_views.TryGetValue(stageId, out UI_ConsoleComponentView view) == false)
            return;

        StageMission[] runtimeMissions = stage.RuntimeMissions;
        if (runtimeMissions == null)
            return;

        for (int i = 0; i < runtimeMissions.Length; i++)
        {
            if (runtimeMissions[i].isMissionSuccess == false)
                continue;

            view.IsSuccess(runtimeMissions[i].missionIndex);
        }
    }

    private List<(int missionIndex, string missionTitle)> BuildMissionData(StageDefinition definition)
    {
        List<(int missionIndex, string missionTitle)> result = new();

        if (definition == null || definition.Missions == null)
            return result;

        for (int i = 0; i < definition.Missions.Length; i++)
        {
            StageMission mission = definition.Missions[i];
            result.Add((mission.missionIndex, mission.missionTitle));
        }

        return result;
    }

    private void ClearAllViews()
    {
        foreach (UI_ConsoleComponentView view in _views.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        _views.Clear();
    }
}
