using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_UIInspectorComponentPrefabCatalog", menuName = "Scriptable Objects/SO_UIInspectorComponentPrefabCatalog")]
public class SO_UIInspectorComponentPrefabCatalog : ScriptableObject
{
    [Serializable]
    public class UIInspectorComponentPrefab
    {
        [SerializeField] private InspectorComponent _componentType;
        [SerializeField] private GameObject _componentPrefab;
        public InspectorComponent ComponentType => _componentType;
        public GameObject ComponentPrefab => _componentPrefab;
    }

    [SerializeField]
    private List<UIInspectorComponentPrefab> _inspectorComponentPrefab;

    public IReadOnlyList<UIInspectorComponentPrefab> InspectorComponentPrefab => _inspectorComponentPrefab;
}

