using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_InspectorPresenter : MonoBehaviour
{
    [SerializeField] private UI_InspectorHeaderView _view;
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private SO_UIInspectorComponentPrefabCatalog _prefabCatalog;

    private readonly List<GameObject> _spawnedSections = new();
    private readonly List<IUIInspectorReferenceSection> _boundReferenceSections = new();

    public event Action<string, InspectorComponent, string, string> ReferenceDropped;

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

            var section = FindSectionComponent(instance);
            if (section == null)
            {
                Debug.LogWarning($"UI_InspectorPresenter: {instance.name}에 IUIInspectorSection 구현체가 없습니다.");
                continue;
            }

            section.Bind(runtimeData.Id, sectionData);

            if (section is IUIInspectorReferenceSection referenceSection)
            {
                referenceSection.ReferenceDropped += HandleReferenceDropped;
                _boundReferenceSections.Add(referenceSection);
            }
        }
    }

    public void Clear()
    {
        ClearSections();
        _view?.Hide();
    }

    private void ClearSections()
    {
        for (int i = 0; i < _boundReferenceSections.Count; i++)
            _boundReferenceSections[i].ReferenceDropped -= HandleReferenceDropped;

        _boundReferenceSections.Clear();

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

    private IUIInspectorSection FindSectionComponent(GameObject instance)
    {
        var behaviours = instance.GetComponents<MonoBehaviour>();

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IUIInspectorSection section)
                return section;
        }

        return null;
    }

    private void HandleReferenceDropped(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        ReferenceDropped?.Invoke(ownerId, inspectorComponent, slotId, targetId);
    }
}
