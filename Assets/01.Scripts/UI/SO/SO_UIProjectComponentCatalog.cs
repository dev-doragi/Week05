using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_UIProjectComponentCatalog", menuName = "Scriptable Objects/SO_UIProjectComponentCatalog")]
public class SO_UIProjectComponentCatalog : ScriptableObject
{
    [Serializable]
    public class UIProjectComponentCatalog
    {
        [SerializeField] private ProejctFileStruct _projectFileStruct;
        [SerializeField] private SO_ComponentData _catalogComponent;
        public ProejctFileStruct ProjectFileStruct => _projectFileStruct;
        public SO_ComponentData CatalogComponent => _catalogComponent;
    }

    [SerializeField]
    private List<UIProjectComponentCatalog> _components;

    public IReadOnlyList<UIProjectComponentCatalog> ComponentCatalogs => _components;
}

