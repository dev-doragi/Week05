using System.Collections.Generic;
using UnityEngine;

public class UI_InGameEditorRuntimeState
{
    private readonly Dictionary<string, UI_InGameEditorRuntimeData> _componentById = new();
    private readonly List<string> _projectItemIds = new();
    private readonly List<string> _hierarchyItemIds = new();

    public string SelectedRuntimeComponentId { get; private set; }
    public string SelectedProjectComponentId { get; private set; }
    public string SelectedHierarchyComponentId { get; private set; }
    public UIEditorScene CurrentScene { get; private set; }

    public void Init(SO_InGameEditorInitialPlacementData initialPlacementData)
    {
        _componentById.Clear();
        _projectItemIds.Clear();
        _hierarchyItemIds.Clear();

        SelectedRuntimeComponentId = null;
        SelectedProjectComponentId = null;
        SelectedHierarchyComponentId = null;
        CurrentScene = UIEditorScene.None;

        if (initialPlacementData == null)
            return;

        bool hasScene = false;

        foreach (var objectEntry in initialPlacementData.Objects)
        {
            if (objectEntry == null || string.IsNullOrEmpty(objectEntry.ObjectId) || objectEntry.ComponentData == null)
                continue;

            var runtimeData = CreateRuntimeData(objectEntry);

            _componentById[runtimeData.Id] = runtimeData;
            RegisterWindow(runtimeData);

            if (hasScene == false && runtimeData.HierarchyData != null)
            {
                CurrentScene = runtimeData.HierarchyData.Scene;
                hasScene = true;
            }
        }

        RefreshAllReferenceDisplayNames();
    }

    public void SetCurrentScene(UIEditorScene scene)
    {
        CurrentScene = scene;
    }

    public void SelectFromProject(string componentId)
    {
        SelectedRuntimeComponentId = componentId;
        SelectedProjectComponentId = componentId;
        SelectedHierarchyComponentId = null;
    }

    public void SelectFromHierarchy(string componentId)
    {
        SelectedRuntimeComponentId = componentId;
        SelectedProjectComponentId = null;
        SelectedHierarchyComponentId = componentId;
    }

    public UI_InGameEditorRuntimeData GetSelectedRuntimeData()
    {
        if (string.IsNullOrEmpty(SelectedRuntimeComponentId))
            return null;

        _componentById.TryGetValue(SelectedRuntimeComponentId, out var data);
        return data;
    }

    public IReadOnlyList<UI_InGameEditorRuntimeData> GetProjectItems()
    {
        var items = new List<UI_InGameEditorRuntimeData>();

        foreach (var id in _projectItemIds)
        {
            if (_componentById.TryGetValue(id, out var data))
                items.Add(data);
        }

        return items;
    }

    public IReadOnlyList<UI_InGameEditorRuntimeData> GetHierarchyItems()
    {
        var items = new List<UI_InGameEditorRuntimeData>();

        foreach (var id in _hierarchyItemIds)
        {
            if (_componentById.TryGetValue(id, out var data) == false)
                continue;

            if (data.HierarchyData == null)
                continue;

            if (data.HierarchyData.Scene != CurrentScene)
                continue;

            items.Add(data);
        }

        return items;
    }

    public List<UI_RuntimeReferenceKey> GetBreakableReferenceKeys()
    {
        var keys = new List<UI_RuntimeReferenceKey>();

        foreach (var pair in _componentById)
        {
            var runtimeData = pair.Value;

            foreach (var section in runtimeData.Sections)
            {
                foreach (var reference in section.References)
                {
                    if (reference.CanSpawnError == false)
                        continue;

                    if (reference.IsCorrect() == false)
                        continue;

                    keys.Add(new UI_RuntimeReferenceKey
                    {
                        OwnerId = runtimeData.Id,
                        InspectorComponent = section.InspectorComponent,
                        SlotId = reference.SlotId
                    });
                }
            }
        }

        return keys;
    }

    public bool TryBreakReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        var reference = FindReference(ownerId, inspectorComponent, slotId);

        if (reference == null || reference.CanSpawnError == false)
            return false;

        reference.CurrentTargetId = null;
        reference.CurrentTargetDisplayName = null;
        return true;
    }

    public bool TryBreakRandomReference(out UI_RuntimeReferenceKey brokenKey)
    {
        brokenKey = null;

        var candidates = GetBreakableReferenceKeys();
        if (candidates.Count == 0)
            return false;

        int randomIndex = Random.Range(0, candidates.Count);
        var candidate = candidates[randomIndex];

        if (TryBreakReference(candidate.OwnerId, candidate.InspectorComponent, candidate.SlotId) == false)
            return false;

        brokenKey = candidate;
        return true;
    }

    public bool TryRestoreReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        var reference = FindReference(ownerId, inspectorComponent, slotId);

        if (reference == null)
            return false;

        reference.CurrentTargetId = reference.ExpectedTargetId;
        reference.CurrentTargetDisplayName = reference.ExpectedTargetDisplayName;
        return true;
    }

    public bool TryAssignReference(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        var reference = FindReference(ownerId, inspectorComponent, slotId);

        if (reference == null)
            return false;

        reference.CurrentTargetId = targetId;
        reference.CurrentTargetDisplayName = ResolveDisplayName(targetId);
        return true;
    }

    public UI_ValidationSummary ValidateAll()
    {
        var summary = new UI_ValidationSummary();

        foreach (var pair in _componentById)
        {
            var runtimeData = pair.Value;

            foreach (var section in runtimeData.Sections)
            {
                foreach (var reference in section.References)
                {
                    var errorType = GetErrorType(reference);
                    if (errorType == UI_ReferenceValidationErrorType.None)
                        continue;

                    summary.Errors.Add(new UI_ReferenceValidationResult
                    {
                        OwnerId = runtimeData.Id,
                        OwnerDisplayName = runtimeData.DisplayName,
                        InspectorComponent = section.InspectorComponent,
                        SlotId = reference.SlotId,
                        SlotLabel = reference.Label,
                        ExpectedTargetId = reference.ExpectedTargetId,
                        CurrentTargetId = reference.CurrentTargetId,
                        ErrorType = errorType
                    });
                }
            }
        }

        return summary;
    }

    private UI_ReferenceValidationErrorType GetErrorType(UI_RuntimeReferenceData reference)
    {
        if (reference == null)
            return UI_ReferenceValidationErrorType.None;

        if (reference.IsMissing())
            return UI_ReferenceValidationErrorType.MissingReference;

        if (reference.IsWrongReference())
            return UI_ReferenceValidationErrorType.WrongReference;

        return UI_ReferenceValidationErrorType.None;
    }

    private UI_InGameEditorRuntimeData CreateRuntimeData(SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        var runtimeData = new UI_InGameEditorRuntimeData
        {
            Id = objectEntry.ObjectId,
            DisplayName = objectEntry.ComponentData.DisplayName,
            SourceData = objectEntry.ComponentData,
        };

        BuildWindowData(runtimeData, objectEntry);
        BuildSections(runtimeData, objectEntry);

        return runtimeData;
    }

    private void BuildWindowData(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        switch (runtimeData.SourceData.ComponentWindow)
        {
            case ComponentWindow.Project:
                runtimeData.ProjectData = new UI_ProjectRuntimeData
                {
                    FileStruct = runtimeData.SourceData.ProjectFileStruct
                };
                break;

            case ComponentWindow.Hierarchy:
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData
                {
                    Scene = objectEntry.Scene
                };
                break;

            case ComponentWindow.Both:
                runtimeData.ProjectData = new UI_ProjectRuntimeData
                {
                    FileStruct = runtimeData.SourceData.ProjectFileStruct
                };
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData
                {
                    Scene = objectEntry.Scene
                };
                break;
        }
    }

    private void BuildSections(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        foreach (var inspectorComponent in objectEntry.ComponentData.InspectorComponents)
        {
            var section = new UI_RuntimeInspectorSectionData
            {
                InspectorComponent = inspectorComponent
            };

            foreach (var referenceEntry in objectEntry.References)
            {
                if (referenceEntry == null || referenceEntry.InspectorComponent != inspectorComponent)
                    continue;

                section.References.Add(new UI_RuntimeReferenceData
                {
                    SlotId = referenceEntry.SlotId,
                    Label = referenceEntry.Label,
                    ExpectedTargetId = referenceEntry.NormalTargetId,
                    ExpectedTargetDisplayName = referenceEntry.NormalTargetId,
                    CurrentTargetId = referenceEntry.NormalTargetId,
                    CurrentTargetDisplayName = referenceEntry.NormalTargetId,
                    CanSpawnError = referenceEntry.CanSpawnError,
                    IsRequired = referenceEntry.IsRequired
                });
            }

            runtimeData.Sections.Add(section);
        }
    }

    private void RegisterWindow(UI_InGameEditorRuntimeData runtimeData)
    {
        if (runtimeData.ProjectData != null && _projectItemIds.Contains(runtimeData.Id) == false)
            _projectItemIds.Add(runtimeData.Id);

        if (runtimeData.HierarchyData != null && _hierarchyItemIds.Contains(runtimeData.Id) == false)
            _hierarchyItemIds.Add(runtimeData.Id);
    }

    private UI_RuntimeReferenceData FindReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        if (_componentById.TryGetValue(ownerId, out var data) == false)
            return null;

        foreach (var section in data.Sections)
        {
            if (section.InspectorComponent != inspectorComponent)
                continue;

            foreach (var reference in section.References)
            {
                if (reference.SlotId == slotId)
                    return reference;
            }
        }

        return null;
    }

    private void RefreshAllReferenceDisplayNames()
    {
        foreach (var pair in _componentById)
        {
            foreach (var section in pair.Value.Sections)
            {
                foreach (var reference in section.References)
                {
                    reference.ExpectedTargetDisplayName = ResolveDisplayName(reference.ExpectedTargetId);
                    reference.CurrentTargetDisplayName = ResolveDisplayName(reference.CurrentTargetId);
                }
            }
        }
    }

    private string ResolveDisplayName(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            return null;

        if (_componentById.TryGetValue(targetId, out var runtimeData))
            return runtimeData.DisplayName;

        return targetId;
    }
}
