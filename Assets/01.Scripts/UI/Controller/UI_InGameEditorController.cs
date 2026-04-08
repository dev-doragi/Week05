using UnityEngine;

public class InGameEditorController : MonoBehaviour
{
    [Header("Initial Data")]
    [SerializeField] private SO_InGameEditorInitialPlacementData _initialPlacementData;

    [Header("Presenter")]
    [SerializeField] private UI_ConsolePresenter _consolePresenter;
    [SerializeField] private UI_ProjectPresenter _projectPresenter;
    [SerializeField] private UI_HierarchyPresenter _hierarchyPresenter;
    [SerializeField] private UI_InspectorPresenter _inspectorPresenter;

    private UI_InGameEditorRuntimeState _runtimeState;

    public UI_InGameEditorRuntimeState EditorRuntimeState => _runtimeState;

    private void Awake()
    {
        _consolePresenter ??= FindFirstObjectByType<UI_ConsolePresenter>();
        _projectPresenter ??= FindFirstObjectByType<UI_ProjectPresenter>();
        _hierarchyPresenter ??= FindFirstObjectByType<UI_HierarchyPresenter>();
        _inspectorPresenter ??= FindFirstObjectByType<UI_InspectorPresenter>();

        if (_projectPresenter != null)
            _projectPresenter.ItemClicked += HandleProjectItemClicked;

        if (_hierarchyPresenter != null)
            _hierarchyPresenter.ItemClicked += HandleHierarchyItemClicked;

        if (_inspectorPresenter != null)
            _inspectorPresenter.ReferenceDropped += HandleReferenceDropped;

        Init();
    }

    private void OnDestroy()
    {
        if (_projectPresenter != null)
            _projectPresenter.ItemClicked -= HandleProjectItemClicked;

        if (_hierarchyPresenter != null)
            _hierarchyPresenter.ItemClicked -= HandleHierarchyItemClicked;

        if (_inspectorPresenter != null)
            _inspectorPresenter.ReferenceDropped -= HandleReferenceDropped;
    }

    public void Init()
    {
        _runtimeState = new UI_InGameEditorRuntimeState();
        _runtimeState.Init(_initialPlacementData);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    [ContextMenu("Start Quiz")]
    public void StartQuiz()
    {
        if (_runtimeState == null)
            return;

        if (_runtimeState.TryBreakRandomReference(out var brokenKey))
        {
            Debug.Log($"[Quiz] Reference broken: owner={brokenKey.OwnerId}, slot={brokenKey.SlotId}");
        }
        else
        {
            Debug.Log("[Quiz] No breakable reference found.");
        }

        RefreshInspector();
        LogValidationSummary(_runtimeState.ValidateAll());
    }

    private void RefreshProject()
    {
        if (_projectPresenter == null)
            return;

        var items = _runtimeState.GetProjectItems();
        _projectPresenter.Render(items, _runtimeState.SelectedProjectComponentId);
    }

    private void RefreshHierarchy()
    {
        if (_hierarchyPresenter == null)
            return;

        var items = _runtimeState.GetHierarchyItems();
        _hierarchyPresenter.Render(items, _runtimeState.SelectedHierarchyComponentId);
    }

    private void RefreshInspector()
    {
        if (_inspectorPresenter == null)
            return;

        var selectedData = _runtimeState.GetSelectedRuntimeData();

        if (selectedData == null)
        {
            _inspectorPresenter.Clear();
            return;
        }

        _inspectorPresenter.Render(selectedData);
    }

    private void HandleProjectItemClicked(string componentId)
    {
        _runtimeState.SelectFromProject(componentId);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    private void HandleHierarchyItemClicked(string componentId)
    {
        _runtimeState.SelectFromHierarchy(componentId);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    private void HandleReferenceDropped(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        bool assigned = _runtimeState.TryAssignReference(ownerId, inspectorComponent, slotId, targetId);

        if (assigned == false)
        {
            Debug.LogWarning($"[Validation] Assign failed: owner={ownerId}, section={inspectorComponent}, slot={slotId}, target={targetId}");
            return;
        }

        RefreshInspector();
        LogValidationSummary(_runtimeState.ValidateAll());
    }

    private void LogValidationSummary(UI_ValidationSummary summary)
    {
        if (summary == null)
            return;

        if (summary.IsSolved)
        {
            Debug.Log("[Validation] All references are correct.");
            return;
        }

        for (int i = 0; i < summary.Errors.Count; i++)
        {
            var error = summary.Errors[i];
            string currentTarget = string.IsNullOrEmpty(error.CurrentTargetId) ? "Empty" : error.CurrentTargetId;

            Debug.Log(
                $"[Validation][{error.ErrorType}] " +
                $"owner={error.OwnerDisplayName}({error.OwnerId}), " +
                $"section={error.InspectorComponent}, " +
                $"slot={error.SlotLabel}({error.SlotId}), " +
                $"expected={error.ExpectedTargetId}, " +
                $"current={currentTarget}");
        }
    }
}
