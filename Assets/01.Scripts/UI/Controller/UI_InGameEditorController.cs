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

    private void HandleReferenceDropped(string ownerId, string slotId, string targetId)
    {
        _runtimeState.TryAssignReference(ownerId, slotId, targetId);
        RefreshInspector();
    }
}
