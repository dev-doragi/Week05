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
            if (objectEntry == null)
                continue;

            if (string.IsNullOrEmpty(objectEntry.ObjectId))
                continue;

            if (objectEntry.ComponentData == null)
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

    public bool TryBreakReference(string ownerId, string slotId)
    {
        var reference = FindReference(ownerId, slotId);

        if (reference == null)
            return false;

        if (reference.CanSpawnError == false)
            return false;

        reference.CurrentTargetId = null;
        return true;
    }

    public bool TryRestoreReference(string ownerId, string slotId)
    {
        var reference = FindReference(ownerId, slotId);

        if (reference == null)
            return false;

        reference.CurrentTargetId = reference.ExpectedTargetId;
        return true;
    }

    public bool TryAssignReference(string ownerId, string slotId, string targetId)
    {
        var reference = FindReference(ownerId, slotId);

        if (reference == null)
            return false;

        reference.CurrentTargetId = targetId;
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
                if (referenceEntry == null)
                    continue;

                if (referenceEntry.InspectorComponent != inspectorComponent)
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

    private void RegisterWindow(UI_InGameEditorRuntimeData runtimeData)
    {
        if (runtimeData.ProjectData != null && _projectItemIds.Contains(runtimeData.Id) == false)
            _projectItemIds.Add(runtimeData.Id);

        if (runtimeData.HierarchyData != null && _hierarchyItemIds.Contains(runtimeData.Id) == false)
            _hierarchyItemIds.Add(runtimeData.Id);
    }

    private UI_RuntimeReferenceData FindReference(string ownerId, string slotId)
    {
        if (_componentById.TryGetValue(ownerId, out var data) == false)
            return null;

        foreach (var section in data.Sections)
        {
            foreach (var reference in section.References)
            {
                if (reference.SlotId == slotId)
                    return reference;
            }
        }

        return null;
    }
}
