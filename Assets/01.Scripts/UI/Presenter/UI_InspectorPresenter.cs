using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_InspectorPresenter : MonoBehaviour
{
    [SerializeField] private UI_InspectorHeaderView _view;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private SO_UIInspectorComponentPrefabCatalog _prefabCatalog;

    private readonly List<GameObject> _spawnedSections = new();

    public event Action<string, string, string> ReferenceDropped;

    public void Render(UI_InGameEditorRuntimeData runtimeData)
    {
        ClearSections();

        if (runtimeData == null)
        {
            _view?.Hide();
            return;
        }

        if (_view == null || _contentRoot == null || _prefabCatalog == null)
            return;

        _view.Show(runtimeData.DisplayName);

        foreach (var inspectorComponent in runtimeData.SourceData.InspectorComponents)
        {
            var prefab = FindPrefab(inspectorComponent);
            if (prefab == null)
            {
                Debug.LogWarning($"UI_InspectorPresenter: {inspectorComponent} 프리팹이 없습니다.");
                continue;
            }

            var sectionData = FindSection(runtimeData, inspectorComponent);
            var instance = Instantiate(prefab, _contentRoot);
            instance.transform.SetAsFirstSibling();
            _spawnedSections.Add(instance);

            BindSection(instance, runtimeData, inspectorComponent, sectionData);
        }
    }

    public void Clear()
    {
        ClearSections();
        _view?.Hide();
    }

    private void ClearSections()
    {
        for (int i = 0; i < _spawnedSections.Count; i++)
        {
            if (_spawnedSections[i] != null)
                Destroy(_spawnedSections[i]);
        }

        _spawnedSections.Clear();
    }

    private GameObject FindPrefab(InspectorComponent inspectorComponent)
    {
        foreach (var entry in _prefabCatalog.InspectorComponentPrefab)
        {
            if (entry.ComponentType == inspectorComponent)
                return entry.ComponentPrefab;
        }

        return null;
    }

    private UI_RuntimeInspectorSectionData FindSection(
        UI_InGameEditorRuntimeData runtimeData,
        InspectorComponent inspectorComponent)
    {
        foreach (var section in runtimeData.Sections)
        {
            if (section.InspectorComponent == inspectorComponent)
                return section;
        }

        return null;
    }

    private void BindSection(
        GameObject instance,
        UI_InGameEditorRuntimeData runtimeData,
        InspectorComponent inspectorComponent,
        UI_RuntimeInspectorSectionData sectionData)
    {
        switch (inspectorComponent)
        {
            case InspectorComponent.PlayerController:
                {
                    var playerControllerSection = instance.GetComponent<UI_InspectorPlayerControllerSection>();
                    if (playerControllerSection != null)
                    {
                        playerControllerSection.ReferenceDropped += HandleReferenceDropped;
                        playerControllerSection.Bind(runtimeData.Id, sectionData);
                    }
                    break;
                }
        }
    }

    private void HandleReferenceDropped(string ownerId, string slotId, string targetId)
    {
        ReferenceDropped?.Invoke(ownerId, slotId, targetId);
    }
}
