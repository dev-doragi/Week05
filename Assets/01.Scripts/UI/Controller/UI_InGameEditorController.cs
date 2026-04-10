using UnityEngine;

public class InGameEditorController : MonoBehaviour
{
    [Header("Initial Data")]
    [SerializeField] private SO_InGameEditorInitialPlacementData _initialPlacementData;

    [Header("Presenter")]
    [SerializeField] private UI_ProjectPresenter _projectPresenter;
    [SerializeField] private UI_HierarchyPresenter _hierarchyPresenter;
    [SerializeField] private UI_InspectorPresenter _inspectorPresenter;
    [SerializeField] private UI_ConsolePresenter _consolePresenter;

    private int _nextPlacedObjectIndex = 1;

    public static UI_InGameEditorRuntimeState EditorRuntimeState;

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

        if (EditorRuntimeState != null)
            EditorRuntimeState.ReferencesChanged -= HandleReferencesChanged;
    }

    public void Init()
    {
        if (EditorRuntimeState != null)
            EditorRuntimeState.ReferencesChanged -= HandleReferencesChanged;

        EditorRuntimeState = new UI_InGameEditorRuntimeState();
        EditorRuntimeState.Init(_initialPlacementData);
        EditorRuntimeState.ReferencesChanged += HandleReferencesChanged;
        _nextPlacedObjectIndex = 1;

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

        RefreshHierarchy();
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

    public void SelectWorldObject(string componentId, UIEditorScene scene)
    {
        if (EditorRuntimeState == null || string.IsNullOrEmpty(componentId))
            return;

        if (scene != UIEditorScene.None)
            EditorRuntimeState.SetCurrentScene(scene);

        SelectHierarchyComponent(componentId);
    }

    public void PlaceDraggedPrefab(SO_UIPlaceablePrefabRecipe recipe, Vector3 worldPosition)
    {
        if (EditorRuntimeState == null || recipe == null || recipe.ComponentData == null || recipe.PlacementPrefab == null)
            return;

        UIEditorScene scene = recipe.DefaultScene != UIEditorScene.None
            ? recipe.DefaultScene
            : EditorRuntimeState.CurrentScene;

        string objectId = CreatePlacedObjectId(recipe.ComponentData);
        GameObject instance = Instantiate(recipe.PlacementPrefab);
        instance.name = objectId;
        instance.transform.position = worldPosition;

        var selectable = instance.GetComponent<GameObjectSelectable>();
        if (selectable == null)
            selectable = instance.AddComponent<GameObjectSelectable>();

        selectable.Bind(objectId, scene);

        bool added = EditorRuntimeState.AddPlacedObject(
            objectId,
            recipe.ComponentData,
            scene,
            recipe.DefaultReferences);

        if (added == false)
        {
            Destroy(instance);
            return;
        }

        if (scene != UIEditorScene.None)
            EditorRuntimeState.SetCurrentScene(scene);

        SelectHierarchyComponent(objectId);
    }

    private void HandleHierarchyItemClicked(string componentId)
    {
        SelectHierarchyComponent(componentId);
    }

    private void SelectHierarchyComponent(string componentId)
    {
        EditorRuntimeState.SelectFromHierarchy(componentId);

        RefreshProject();
        RefreshHierarchy();
        RefreshInspector();
    }

    private string CreatePlacedObjectId(SO_ComponentData componentData)
    {
        string baseId = componentData != null && string.IsNullOrEmpty(componentData.ComponentId) == false
            ? componentData.ComponentId
            : "PlacedObject";

        while (true)
        {
            string objectId = $"{baseId}_Instance_{_nextPlacedObjectIndex++:000}";

            if (EditorRuntimeState.ContainsRuntimeObject(objectId) == false)
                return objectId;
        }
    }

    private void HandleReferenceDropped(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        bool assigned = EditorRuntimeState.TryAssignReference(ownerId, inspectorComponent, slotId, targetId);

        if (assigned == false)
        {
            Debug.LogWarning($"[Validation] Assign failed: owner={ownerId}, section={inspectorComponent}, slot={slotId}, target={targetId}");
            return;
        }

        LogValidationSummary(EditorRuntimeState.ValidateAll());
    }

    private void HandleReferencesChanged()
    {
        RefreshHierarchy();
        RefreshInspector();
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
