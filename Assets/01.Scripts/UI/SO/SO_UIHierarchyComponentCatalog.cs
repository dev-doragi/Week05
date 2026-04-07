using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_UIHierarchyComponentCatalog", menuName = "Scriptable Objects/SO_UIHierarchyComponentCatalog")]
public class SO_UIHierarchyComponentCatalog : ScriptableObject
{
    [Serializable]
    public class UIHierarchyComponentCatalog
    {
        [SerializeField] private UIEditorScene _uiEditorScene;
        [SerializeField] private List<SO_ComponentData> _catalogComponents = new();
        public UIEditorScene UIEditorScene => _uiEditorScene;
        public List<SO_ComponentData> CatalogComponents => _catalogComponents;
    }

    [SerializeField]
    private List<UIHierarchyComponentCatalog> _components;

    public IReadOnlyList<UIHierarchyComponentCatalog> ComponentCatalogs => _components;
}

public enum UIEditorScene
{

}