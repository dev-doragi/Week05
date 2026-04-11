using System.Collections.Generic;
using UnityEngine;

public class UI_ConsolePresenter : MonoBehaviour
{
    [Header("Initial Data")]
    [SerializeField] private List<StageDefinition> _stageDefinitions = new();

    [Header("View")]
    [SerializeField] private UI_ConsoleComponentView _viewPrefab;
    [SerializeField] private Transform _viewParent;

    [Header("Scroll")]
    [SerializeField] private UI_ScrollFocusController _scrollFocusController;

    private readonly Dictionary<string, UI_ConsoleComponentView> _viewsByStageId = new();
    private readonly Dictionary<string, HashSet<int>> _appliedMissionIndicesByStageId = new();

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

        if (_viewsByStageId.ContainsKey(definition.StageId))
            return;

        Transform parent = _viewParent != null ? _viewParent : transform;
        UI_ConsoleComponentView view = Instantiate(_viewPrefab, parent);

        List<(int missionIndex, string missionTitle)> missionData = BuildMissionData(definition);

        view.Init(
            definition.StageId,
            definition.StageTitle,
            missionData,
            missionData.Count);

        _viewsByStageId.Add(definition.StageId, view);
        _appliedMissionIndicesByStageId.Add(definition.StageId, new HashSet<int>());
    }

    private void ApplyCurrentRuntimeStates()
    {
        Stage[] stages = FindObjectsByType<Stage>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < stages.Length; i++)
        {
            ApplyRuntimeState(stages[i], shouldFocusOnNewSuccess: false);
        }
    }

    private void HandleStageChanged(Stage changedStage)
    {
        ApplyRuntimeState(changedStage, shouldFocusOnNewSuccess: true);
    }

    private void ApplyRuntimeState(Stage stage, bool shouldFocusOnNewSuccess)
    {
        if (TryGetStageContext(stage, out string stageId, out UI_ConsoleComponentView view) == false)
            return;

        if (_appliedMissionIndicesByStageId.TryGetValue(stageId, out HashSet<int> appliedMissionIndices) == false)
            return;

        StageMission[] runtimeMissions = stage.RuntimeMissions;
        if (runtimeMissions == null)
            return;

        bool hasNewSuccess = false;

        for (int i = 0; i < runtimeMissions.Length; i++)
        {
            StageMission mission = runtimeMissions[i];

            if (mission.isMissionSuccess == false)
                continue;

            if (appliedMissionIndices.Add(mission.missionIndex) == false)
                continue;

            view.IsSuccess(mission.missionIndex);
            hasNewSuccess = true;
        }

        if (shouldFocusOnNewSuccess && hasNewSuccess)
        {
            FocusView(view);
        }
    }

    private bool TryGetStageContext(
        Stage stage,
        out string stageId,
        out UI_ConsoleComponentView view)
    {
        stageId = null;
        view = null;

        if (stage == null)
            return false;

        if (stage.StageSO == null)
            return false;

        stageId = stage.StageSO.StageId;

        if (string.IsNullOrEmpty(stageId))
            return false;

        if (_viewsByStageId.TryGetValue(stageId, out view) == false)
            return false;

        return true;
    }

    private void FocusView(UI_ConsoleComponentView view)
    {
        if (_scrollFocusController == null)
            return;

        if (view == null)
            return;

        RectTransform targetRect = view.transform as RectTransform;
        if (targetRect == null)
            return;

        _scrollFocusController.Focus(targetRect);
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
        foreach (UI_ConsoleComponentView view in _viewsByStageId.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        _viewsByStageId.Clear();
        _appliedMissionIndicesByStageId.Clear();
    }
}
