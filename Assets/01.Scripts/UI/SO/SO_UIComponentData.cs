using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_ComponentData", menuName = "Scriptable Objects/SO_ComponentData")]
public class SO_ComponentData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _componentId;
    [SerializeField] private string _displayName;
    [SerializeField] private Sprite _iconSprite;

    [Header("Window")]
    [SerializeField] private ComponentWindow _componentWindow;
    [SerializeField] private ProejctFileStruct _projectFileStruct;

    [Header("Project Drag")]
    [SerializeField] private UIProjectDragType _projectDragType = UIProjectDragType.ReferenceTarget;
    [SerializeField] private SO_UIPlaceablePrefabRecipe _placeablePrefabRecipe;

    [Header("Inspector")]
    [SerializeField] private List<InspectorComponent> _inspectorComponents = new();

    public string ComponentId => _componentId;
    public string DisplayName => _displayName;
    public Sprite IconSprite => _iconSprite;
    public ComponentWindow ComponentWindow => _componentWindow;
    public ProejctFileStruct ProjectFileStruct => _projectFileStruct;
    public UIProjectDragType ProjectDragType => _projectDragType;
    public SO_UIPlaceablePrefabRecipe PlaceablePrefabRecipe => _placeablePrefabRecipe;
    public IReadOnlyList<InspectorComponent> InspectorComponents => _inspectorComponents;
}
