using System.Collections.Generic;
using UnityEngine;

public class UI_InGameEditorRuntimeState
{
    private readonly Dictionary<string, UI_InGameEditorRuntimeData> _componentById = new();
    private readonly List<string> _projectItemIds = new();
    private readonly List<string> _hierarchyItemIds = new();

    public event System.Action ReferencesChanged;
    public event System.Action<UI_RuntimeReferenceKey> ReferenceBroken;
    public event System.Action<UI_RuntimeReferenceKey> ReferenceSolved;
    public event System.Action<string, bool> OwnerBlockStateChanged;

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

    public bool ContainsRuntimeObject(string objectId)
    {
        if (string.IsNullOrEmpty(objectId))
            return false;

        return _componentById.ContainsKey(objectId);
    }

    public bool HasAnyReferenceError(string ownerId)
    {
        if (_componentById.TryGetValue(ownerId, out var runtimeData) == false)
            return false;

        return runtimeData.HasReferenceError();
    }

    public bool HasMissingReference(string ownerId)
    {
        if (_componentById.TryGetValue(ownerId, out var runtimeData) == false)
            return false;

        foreach (var section in runtimeData.Sections)
        {
            if (section == null)
                continue;

            foreach (var reference in section.References)
            {
                if (reference == null)
                    continue;

                if (reference.GetErrorType() == UI_ReferenceValidationErrorType.MissingReference)
                    return true;
            }
        }

        return false;
    }

    public bool IsOwnerBlocked(string ownerId)
    {
        return HasAnyReferenceError(ownerId);
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

            if (data.HierarchyData == null || data.HierarchyData.Scene != CurrentScene)
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
                    if (!reference.CanSpawnError || !reference.IsCorrect())
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
        bool ownerWasBlocked = IsOwnerBlocked(ownerId);
        var reference = FindReference(ownerId, inspectorComponent, slotId);
        if (reference == null || !reference.CanSpawnError)
            return false;

        bool changed =
            !string.IsNullOrEmpty(reference.CurrentTargetId) ||
            !string.IsNullOrEmpty(reference.CurrentTargetDisplayName);

        reference.CurrentTargetId = null;
        reference.CurrentTargetDisplayName = null;

        if (changed)
            NotifyReferenceBroken(ownerId, inspectorComponent, slotId);

        NotifyOwnerBlockStateChangedIfNeeded(ownerId, ownerWasBlocked);

        if (changed)
            NotifyReferencesChanged();

        return true;
    }

    public bool TryBreakRandomReference(out UI_RuntimeReferenceKey brokenKey)
    {
        brokenKey = null;

        var candidates = GetBreakableReferenceKeys();
        if (candidates.Count == 0)
            return false;

        var candidate = candidates[Random.Range(0, candidates.Count)];
        if (!TryBreakReference(candidate.OwnerId, candidate.InspectorComponent, candidate.SlotId))
            return false;

        brokenKey = candidate;
        return true;
    }

    public bool TryBreakReferenceByComponent(InspectorComponent component, out UI_RuntimeReferenceKey brokenKey)
    {
        brokenKey = null;

        foreach (var pair in _componentById)
        {
            var runtimeData = pair.Value;

            foreach (var section in runtimeData.Sections)
            {
                if (section == null || section.InspectorComponent != component)
                    continue;

                foreach (var reference in section.References)
                {
                    if (reference == null)
                        continue;

                    if (!reference.CanSpawnError || !reference.IsCorrect())
                        continue;

                    if (!TryBreakReference(runtimeData.Id, component, reference.SlotId))
                        return false;

                    brokenKey = new UI_RuntimeReferenceKey
                    {
                        OwnerId = runtimeData.Id,
                        InspectorComponent = component,
                        SlotId = reference.SlotId
                    };

                    return true;
                }
            }
        }

        return false;
    }

    public bool TryRestoreReference(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        return RestoreReferenceCore(ownerId, inspectorComponent, slotId, notifyReferencesChanged: true);
    }

    public int RestoreAllInvalidReferences()
    {
        var invalidKeys = new List<UI_RuntimeReferenceKey>();

        foreach (var pair in _componentById)
        {
            var runtimeData = pair.Value;
            if (runtimeData == null)
                continue;

            foreach (var section in runtimeData.Sections)
            {
                if (section == null)
                    continue;

                foreach (var reference in section.References)
                {
                    if (reference == null || !reference.HasError())
                        continue;

                    invalidKeys.Add(new UI_RuntimeReferenceKey
                    {
                        OwnerId = runtimeData.Id,
                        InspectorComponent = section.InspectorComponent,
                        SlotId = reference.SlotId
                    });
                }
            }
        }

        int restoredCount = 0;

        foreach (var key in invalidKeys)
        {
            if (RestoreReferenceCore(
                key.OwnerId,
                key.InspectorComponent,
                key.SlotId,
                notifyReferencesChanged: false))
            {
                restoredCount++;
            }
        }

        if (restoredCount > 0)
            NotifyReferencesChanged();

        return restoredCount;
    }

    public bool TryAssignReference(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        bool ownerWasBlocked = IsOwnerBlocked(ownerId);
        var reference = FindReference(ownerId, inspectorComponent, slotId);
        if (reference == null)
            return false;

        bool hadError = reference.HasError();
        string targetDisplayName = ResolveDisplayName(targetId);
        bool changed =
            reference.CurrentTargetId != targetId ||
            reference.CurrentTargetDisplayName != targetDisplayName;

        reference.CurrentTargetId = targetId;
        reference.CurrentTargetDisplayName = targetDisplayName;

        NotifyOwnerBlockStateChangedIfNeeded(ownerId, ownerWasBlocked);

        if (changed)
            NotifyReferencesChanged();

        NotifyReferenceSolvedIfNeeded(hadError, ownerId, inspectorComponent, slotId, reference);
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
                    var errorType = reference.GetErrorType();
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

    public bool AddPlacedObject(
        string objectId,
        SO_ComponentData componentData,
        UIEditorScene scene,
        IReadOnlyList<UI_ReferenceTemplateData> defaultReferences)
    {
        if (string.IsNullOrEmpty(objectId) || componentData == null)
            return false;

        if (_componentById.ContainsKey(objectId))
            return false;

        var runtimeData = CreatePlacedRuntimeData(objectId, componentData, scene, defaultReferences);
        _componentById[runtimeData.Id] = runtimeData;

        if (!_hierarchyItemIds.Contains(runtimeData.Id))
            _hierarchyItemIds.Add(runtimeData.Id);

        RefreshReferenceDisplayNames(runtimeData);
        return true;
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

    private UI_InGameEditorRuntimeData CreatePlacedRuntimeData(
        string objectId,
        SO_ComponentData componentData,
        UIEditorScene scene,
        IReadOnlyList<UI_ReferenceTemplateData> defaultReferences)
    {
        var runtimeData = new UI_InGameEditorRuntimeData
        {
            Id = objectId,
            DisplayName = componentData.DisplayName,
            SourceData = componentData,
            HierarchyData = new UI_HierarchyRuntimeData { Scene = scene }
        };

        BuildPlacedSections(runtimeData, defaultReferences);
        return runtimeData;
    }

    private void BuildWindowData(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        var data = runtimeData.SourceData;

        switch (data.ComponentWindow)
        {
            case ComponentWindow.Project:
                runtimeData.ProjectData = new UI_ProjectRuntimeData { FileStruct = data.ProjectFileStruct };
                break;

            case ComponentWindow.Hierarchy:
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData { Scene = objectEntry.Scene };
                break;

            case ComponentWindow.Both:
                runtimeData.ProjectData = new UI_ProjectRuntimeData { FileStruct = data.ProjectFileStruct };
                runtimeData.HierarchyData = new UI_HierarchyRuntimeData { Scene = objectEntry.Scene };
                break;
        }
    }

    private void BuildSections(UI_InGameEditorRuntimeData runtimeData, SO_InGameEditorInitialPlacementData.ObjectEntry objectEntry)
    {
        foreach (var inspectorComponent in objectEntry.ComponentData.InspectorComponents)
        {
            var section = new UI_RuntimeInspectorSectionData { InspectorComponent = inspectorComponent };

            foreach (var referenceEntry in objectEntry.References)
            {
                if (referenceEntry == null || referenceEntry.InspectorComponent != inspectorComponent)
                    continue;

                section.References.Add(new UI_RuntimeReferenceData
                {
                    SlotId = referenceEntry.SlotId,
                    Label = referenceEntry.Label,
                    ExpectedTargetId = referenceEntry.NormalTargetId,
                    CurrentTargetId = referenceEntry.NormalTargetId,
                    CanSpawnError = referenceEntry.CanSpawnError,
                    IsRequired = referenceEntry.IsRequired
                });
            }

            runtimeData.Sections.Add(section);
        }
    }

    private void BuildPlacedSections(
        UI_InGameEditorRuntimeData runtimeData,
        IReadOnlyList<UI_ReferenceTemplateData> defaultReferences)
    {
        if (runtimeData == null || runtimeData.SourceData == null)
            return;

        foreach (var inspectorComponent in runtimeData.SourceData.InspectorComponents)
        {
            var section = new UI_RuntimeInspectorSectionData { InspectorComponent = inspectorComponent };

            if (defaultReferences != null)
            {
                foreach (var referenceTemplate in defaultReferences)
                {
                    if (referenceTemplate == null || referenceTemplate.InspectorComponent != inspectorComponent)
                        continue;

                    section.References.Add(new UI_RuntimeReferenceData
                    {
                        SlotId = referenceTemplate.SlotId,
                        Label = referenceTemplate.Label,
                        ExpectedTargetId = referenceTemplate.DefaultTargetId,
                        CurrentTargetId = referenceTemplate.DefaultTargetId,
                        CanSpawnError = referenceTemplate.CanSpawnError,
                        IsRequired = referenceTemplate.IsRequired
                    });
                }
            }

            runtimeData.Sections.Add(section);
        }
    }

    private void RegisterWindow(UI_InGameEditorRuntimeData runtimeData)
    {
        if (runtimeData.ProjectData != null && !_projectItemIds.Contains(runtimeData.Id))
            _projectItemIds.Add(runtimeData.Id);

        if (runtimeData.HierarchyData != null && !_hierarchyItemIds.Contains(runtimeData.Id))
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

    private bool RestoreReferenceCore(
        string ownerId,
        InspectorComponent inspectorComponent,
        string slotId,
        bool notifyReferencesChanged)
    {
        bool ownerWasBlocked = IsOwnerBlocked(ownerId);
        var reference = FindReference(ownerId, inspectorComponent, slotId);
        if (reference == null)
            return false;

        bool hadError = reference.HasError();
        bool changed =
            reference.CurrentTargetId != reference.ExpectedTargetId ||
            reference.CurrentTargetDisplayName != reference.ExpectedTargetDisplayName;

        reference.CurrentTargetId = reference.ExpectedTargetId;
        reference.CurrentTargetDisplayName = reference.ExpectedTargetDisplayName;

        NotifyOwnerBlockStateChangedIfNeeded(ownerId, ownerWasBlocked);

        if (changed && notifyReferencesChanged)
            NotifyReferencesChanged();

        NotifyReferenceSolvedIfNeeded(hadError, ownerId, inspectorComponent, slotId, reference);
        return true;
    }

    private void RefreshAllReferenceDisplayNames()
    {
        foreach (var pair in _componentById)
        {
            RefreshReferenceDisplayNames(pair.Value);
        }
    }

    private void RefreshReferenceDisplayNames(UI_InGameEditorRuntimeData runtimeData)
    {
        if (runtimeData == null)
            return;

        foreach (var section in runtimeData.Sections)
        {
            foreach (var reference in section.References)
            {
                reference.ExpectedTargetDisplayName = ResolveDisplayName(reference.ExpectedTargetId);
                reference.CurrentTargetDisplayName = ResolveDisplayName(reference.CurrentTargetId);
            }
        }
    }

    private string ResolveDisplayName(string targetId)
    {
        if (string.IsNullOrEmpty(targetId))
            return null;

        return _componentById.TryGetValue(targetId, out var runtimeData) ? runtimeData.DisplayName : targetId;
    }

    private void NotifyReferenceSolvedIfNeeded(
        bool hadError,
        string ownerId,
        InspectorComponent inspectorComponent,
        string slotId,
        UI_RuntimeReferenceData reference)
    {
        if (!hadError || reference == null || !reference.IsCorrect())
            return;

        ReferenceSolved?.Invoke(new UI_RuntimeReferenceKey
        {
            OwnerId = ownerId,
            InspectorComponent = inspectorComponent,
            SlotId = slotId
        });
    }

    private void NotifyReferenceBroken(string ownerId, InspectorComponent inspectorComponent, string slotId)
    {
        ReferenceBroken?.Invoke(new UI_RuntimeReferenceKey
        {
            OwnerId = ownerId,
            InspectorComponent = inspectorComponent,
            SlotId = slotId
        });
    }

    private void NotifyOwnerBlockStateChangedIfNeeded(string ownerId, bool previousBlockedState)
    {
        if (string.IsNullOrEmpty(ownerId))
            return;

        bool currentBlockedState = IsOwnerBlocked(ownerId);
        if (previousBlockedState == currentBlockedState)
            return;

        OwnerBlockStateChanged?.Invoke(ownerId, currentBlockedState);
    }

    private void NotifyReferencesChanged()
    {
        ReferencesChanged?.Invoke();
    }
}
