using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_InGameEditorPlacementCatalog", menuName = "Scriptable Objects/InGameEditor/Placement Catalog")]
public class SO_InGameEditorPlacementCatalog : ScriptableObject
{
    [SerializeField] private List<SO_InGameEditorPlacementObject> _objects = new();
    public IReadOnlyList<SO_InGameEditorPlacementObject> Objects => _objects;
}


[CreateAssetMenu(fileName = "SO_InGameEditorPlacementObject", menuName = "Scriptable Objects/InGameEditor/Placement Object")]
public class SO_InGameEditorPlacementObject : ScriptableObject
{
    [SerializeField] private string _objectId;
    [SerializeField] private SO_ComponentData _componentData;
    [SerializeField] private UIEditorScene _scene;
    [SerializeField] private List<InGameEditorPlacementReferenceEntry> _references = new();

    public string ObjectId => _objectId;
    public SO_ComponentData ComponentData => _componentData;
    public UIEditorScene Scene => _scene;
    public IReadOnlyList<InGameEditorPlacementReferenceEntry> References => _references;
}

[System.Serializable]
public class InGameEditorPlacementReferenceEntry
{
    [SerializeField] private InspectorComponent _inspectorComponent;
    [SerializeField] private string _slotId;
    [SerializeField] private string _label = "reference";
    [SerializeField] private string _normalTargetId;
    [SerializeField] private bool _canSpawnError = true;
    [SerializeField] private bool _isRequired = true;

    public InspectorComponent InspectorComponent => _inspectorComponent;
    public string SlotId => _slotId;
    public string Label => _label;
    public string NormalTargetId => _normalTargetId;
    public bool CanSpawnError => _canSpawnError;
    public bool IsRequired => _isRequired;
}