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
        [SerializeField] private List<SO_ComponentData> _catalogComponents = new();
        public ProejctFileStruct ProjectFileStruct => _projectFileStruct;
        public List<SO_ComponentData> CatalogComponents => _catalogComponents;
    }

    [SerializeField]
    private List<UIProjectComponentCatalog> _components;

    public IReadOnlyList<UIProjectComponentCatalog> ComponentCatalogs => _components;
}

public enum ProejctFileStruct
{

}