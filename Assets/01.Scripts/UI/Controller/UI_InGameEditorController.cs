using UnityEngine;

public class InGameEditorController : MonoBehaviour
{
    [Header("Initial Data")]
    [SerializeField] private SO_InGameEditorInitialPlacementData _initialPlacementData;

    [Header("Presenter")]
    [SerializeField] private UI_ProjectPresenter _projectPresenter;
    [SerializeField] private UI_HierarchyPresenter _hierarchyPresenter;
    [SerializeField] private UI_InspectorPresenter _inspectorPresenter;

    public static UI_InGameEditorRuntimeState EditorRuntimeState { get; private set; }

    private void Awake()
    {
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
        EditorRuntimeState = new UI_InGameEditorRuntimeState();
        EditorRuntimeState.Init(_initialPlacementData);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    [ContextMenu("Start Quiz")]
    public void StartQuiz()
    {
        if (EditorRuntimeState == null)
            return;

        if (EditorRuntimeState.TryBreakRandomReference(out var brokenKey))
        {
            Debug.Log($"[Quiz] Reference broken: owner={brokenKey.OwnerId}, slot={brokenKey.SlotId}");
        }
        else
        {
            Debug.Log("[Quiz] No breakable reference found.");
        }

        RefreshInspector();
        LogValidationSummary(EditorRuntimeState.ValidateAll());
    }

    private void RefreshProject()
    {
        if (_projectPresenter == null)
            return;

        var items = EditorRuntimeState.GetProjectItems();
        _projectPresenter.Render(items, EditorRuntimeState.SelectedProjectComponentId);
    }

    private void RefreshHierarchy()
    {
        if (_hierarchyPresenter == null)
            return;

        var items = EditorRuntimeState.GetHierarchyItems();
        _hierarchyPresenter.Render(items, EditorRuntimeState.SelectedHierarchyComponentId);
    }

    private void RefreshInspector()
    {
        if (_inspectorPresenter == null)
            return;

        var selectedData = EditorRuntimeState.GetSelectedRuntimeData();

        if (selectedData == null)
        {
            _inspectorPresenter.Clear();
            return;
        }

        _inspectorPresenter.Render(selectedData);
    }

    private void HandleProjectItemClicked(string componentId)
    {
        EditorRuntimeState.SelectFromProject(componentId);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    private void HandleHierarchyItemClicked(string componentId)
    {
        EditorRuntimeState.SelectFromHierarchy(componentId);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    private void HandleReferenceDropped(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        bool assigned = EditorRuntimeState.TryAssignReference(ownerId, inspectorComponent, slotId, targetId);

        if (assigned == false)
        {
            Debug.LogWarning($"[Validation] Assign failed: owner={ownerId}, section={inspectorComponent}, slot={slotId}, target={targetId}");
            return;
        }

        RefreshInspector();
        LogValidationSummary(EditorRuntimeState.ValidateAll());
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
