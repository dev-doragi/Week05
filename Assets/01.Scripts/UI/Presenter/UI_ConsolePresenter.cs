using System.Collections.Generic;
using DG.Tweening;
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
    private readonly Dictionary<string, HashSet<int>> _appliedMissionSlotsByStageId = new();
    private readonly HashSet<string> _destroyingStageIds = new();

    private void Awake()
    {
        CreateAllViewsFromDefinitions();
    }

    private void Start()
    {
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

        List<string> missionTitles = BuildMissionTitles(definition);

        view.Init(
            definition.StageId,
            definition.StageTitle,
            missionTitles);

        _viewsByStageId.Add(definition.StageId, view);
        _appliedMissionSlotsByStageId.Add(definition.StageId, new HashSet<int>());
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

        if (_appliedMissionSlotsByStageId.TryGetValue(stageId, out HashSet<int> appliedMissionSlots) == false)
            return;

        if (TryGetReadyRuntimeMissions(stage, out StageMission[] runtimeMissions) == false)
            return;

        bool hasNewSuccess = ApplyMissionSuccesses(view, runtimeMissions, appliedMissionSlots);

        if (shouldFocusOnNewSuccess && hasNewSuccess)
        {
            FocusView(view);
        }

        if (IsStageCompleted(runtimeMissions) == false)
            return;

        if (_destroyingStageIds.Add(stageId) == false)
            return;

        StartDestroySequence(stageId, view);
    }

    private bool TryGetReadyRuntimeMissions(Stage stage, out StageMission[] runtimeMissions)
    {
        runtimeMissions = null;

        if (stage == null)
            return false;

        if (stage.StageSO == null)
            return false;

        StageMission[] definedMissions = stage.StageSO.Missions;
        if (definedMissions == null)
            return false;

        runtimeMissions = stage.RuntimeMissions;
        if (runtimeMissions == null)
            return false;

        // Stage.Awake()가 아직 안 돌면 runtime 은 보통 빈 배열이다.
        if (runtimeMissions.Length != definedMissions.Length)
            return false;

        // 빈 배열은 완료 상태가 아니라 "아직 준비 안 됨"으로 본다.
        if (runtimeMissions.Length == 0)
            return false;

        return true;
    }

    private bool ApplyMissionSuccesses(
        UI_ConsoleComponentView view,
        StageMission[] runtimeMissions,
        HashSet<int> appliedMissionSlots)
    {
        bool hasNewSuccess = false;

        for (int missionSlot = 0; missionSlot < runtimeMissions.Length; missionSlot++)
        {
            StageMission mission = runtimeMissions[missionSlot];

            if (mission.isMissionSuccess == false)
                continue;

            if (appliedMissionSlots.Add(missionSlot) == false)
                continue;

            view.TrySetMissionSuccess(missionSlot);
            hasNewSuccess = true;
        }

        return hasNewSuccess;
    }

    private bool IsStageCompleted(StageMission[] runtimeMissions)
    {
        if (runtimeMissions == null)
            return false;

        if (runtimeMissions.Length == 0)
            return false;

        for (int i = 0; i < runtimeMissions.Length; i++)
        {
            if (runtimeMissions[i].isMissionSuccess == false)
                return false;
        }

        return true;
    }

    private void StartDestroySequence(string stageId, UI_ConsoleComponentView view)
    {
        if (view == null)
        {
            DestroyView(stageId, null);
            return;
        }

        Sequence completeSequence = view.CreateCompleteSequence();

        completeSequence
            .OnComplete(() => DestroyView(stageId, view))
            .Play();
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

    private List<string> BuildMissionTitles(StageDefinition definition)
    {
        List<string> result = new();

        if (definition == null || definition.Missions == null)
            return result;

        for (int i = 0; i < definition.Missions.Length; i++)
        {
            result.Add(definition.Missions[i].missionTitle);
        }

        return result;
    }

    private void DestroyView(string stageId, UI_ConsoleComponentView view)
    {
        _viewsByStageId.Remove(stageId);
        _appliedMissionSlotsByStageId.Remove(stageId);
        _destroyingStageIds.Remove(stageId);

        if (view != null)
            Destroy(view.gameObject);
    }

    private void ClearAllViews()
    {
        foreach (UI_ConsoleComponentView view in _viewsByStageId.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        _viewsByStageId.Clear();
        _appliedMissionSlotsByStageId.Clear();
        _destroyingStageIds.Clear();
    }
}
