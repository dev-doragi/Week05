using System.Collections.Generic;
using UnityEngine;

public class UI_ConsolePresenter : MonoBehaviour
{
    [Header("Create One View Per Stage")]
    [SerializeField] private UI_ConsoleComponentView _viewPrefab;
    [SerializeField] private Transform _viewParent;

    [Header("Optional Manual Stage Order")]
    [SerializeField] private List<Stage> _stages = new();

    // Stage 하나당 Console View 하나를 연결해 둡니다.
    private readonly Dictionary<Stage, UI_ConsoleComponentView> _viewByStage = new();

    private void Awake()
    {
        CollectStages();
        CreateAllViews();
    }

    private void OnEnable()
    {
        Stage.OnStageChanged += HandleStageChanged;
        RefreshAllRuntimeStates();
    }

    private void OnDisable()
    {
        Stage.OnStageChanged -= HandleStageChanged;
    }

    // 필요하면 외부에서 다시 전체 생성/초기화할 때 호출
    public void RenderCurrentStage()
    {
        RebuildAllViews();
    }

    // 특정 Stage 하나만 다시 그리고 싶을 때 사용
    public void Render(Stage stage)
    {
        if (stage == null)
            return;

        EnsureStageRegistered(stage);
        CreateViewForStage(stage);
        DrawDefinition(stage);
        UpdateMissionState(stage);
    }

    // StageDefinition만 받는 방식은 현재 구조와 맞지 않아서 권장하지 않습니다.
    // Stage 여러 개를 관리해야 하므로, 어떤 Stage의 View인지 알아야 하기 때문입니다.
    public void Render(StageDefinition stageDefinition)
    {
        Debug.LogWarning("[UI_ConsolePresenter] Render(StageDefinition) is not used in multi-stage mode. Use Render(Stage) instead.");
    }

    // 특정 Stage의 특정 미션만 반영하고 싶을 때 사용
    public void SetMissionSuccess(Stage stage, int missionArrayIndex)
    {
        if (stage == null)
            return;

        if (_viewByStage.TryGetValue(stage, out var view) == false)
            return;

        StageMission[] runtimeMissions = stage.RuntimeMissions;
        if (runtimeMissions == null)
            return;

        if (missionArrayIndex < 0 || missionArrayIndex >= runtimeMissions.Length)
            return;

        if (runtimeMissions[missionArrayIndex].isMissionSuccess == false)
            return;

        view.IsSuccess(runtimeMissions[missionArrayIndex].missionIndex);
    }

    public void Clear()
    {
        foreach (var pair in _viewByStage)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        _viewByStage.Clear();
    }

    private void RebuildAllViews()
    {
        Clear();
        CollectStages();
        CreateAllViews();
        RefreshAllRuntimeStates();
    }

    private void CollectStages()
    {
        if (_stages == null)
            _stages = new List<Stage>();

        _stages.RemoveAll(stage => stage == null);

        // 인스펙터에 직접 넣어뒀다면 그 순서를 그대로 사용합니다.
        if (_stages.Count > 0)
            return;

        Stage[] foundStages = FindObjectsByType<Stage>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < foundStages.Length; i++)
        {
            if (foundStages[i] == null)
                continue;

            _stages.Add(foundStages[i]);
        }
    }

    private void EnsureStageRegistered(Stage stage)
    {
        if (stage == null)
            return;

        if (_stages.Contains(stage))
            return;

        _stages.Add(stage);
    }

    private void CreateAllViews()
    {
        if (_viewPrefab == null)
        {
            Debug.LogWarning("[UI_ConsolePresenter] View Prefab is missing.");
            return;
        }

        for (int i = 0; i < _stages.Count; i++)
        {
            CreateViewForStage(_stages[i]);
        }
    }

    private void CreateViewForStage(Stage stage)
    {
        if (stage == null)
            return;

        if (_viewByStage.ContainsKey(stage))
            return;

        if (stage.StageSO == null)
            return;

        Transform parent = _viewParent != null ? _viewParent : transform;
        UI_ConsoleComponentView view = Instantiate(_viewPrefab, parent);

        _viewByStage.Add(stage, view);
        DrawDefinition(stage);
    }

    private void DrawDefinition(Stage stage)
    {
        if (stage == null)
            return;

        if (_viewByStage.TryGetValue(stage, out var view) == false)
            return;

        StageDefinition definition = stage.StageSO;
        if (definition == null)
            return;

        List<(int missionIndex, string missionTitle)> missionData = BuildMissionData(definition);

        view.Init(
            definition.StageId,
            definition.StageTitle,
            missionData,
            missionData.Count);
    }

    private void RefreshAllRuntimeStates()
    {
        for (int i = 0; i < _stages.Count; i++)
        {
            UpdateMissionState(_stages[i]);
        }
    }

    private void UpdateMissionState(Stage stage)
    {
        if (stage == null)
            return;

        if (_viewByStage.TryGetValue(stage, out var view) == false)
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

    private List<(int missionIndex, string missionTitle)> BuildMissionData(StageDefinition stageDefinition)
    {
        List<(int missionIndex, string missionTitle)> missionData = new();

        if (stageDefinition == null || stageDefinition.Missions == null)
            return missionData;

        foreach (StageMission mission in stageDefinition.Missions)
        {
            missionData.Add((mission.missionIndex, mission.missionTitle));
        }

        return missionData;
    }

    private void HandleStageChanged(Stage changedStage)
    {
        if (changedStage == null)
            return;

        CreateViewForStage(changedStage);
        UpdateMissionState(changedStage);
    }
}
