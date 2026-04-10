using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_InGameEditorInitialPlacementData", menuName = "Scriptable Objects/SO_InGameEditorInitialPlacementData")]
public class SO_InGameEditorInitialPlacementData : ScriptableObject
{
    [Serializable]
    public class ObjectEntry
    {
        [SerializeField] private string _objectId;
        [SerializeField] private SO_ComponentData _componentData;
        [SerializeField] private UIEditorScene _scene;
        [SerializeField] private List<ReferenceEntry> _references = new();

        public string ObjectId => _objectId;
        public SO_ComponentData ComponentData => _componentData;
        public UIEditorScene Scene => _scene;
        public IReadOnlyList<ReferenceEntry> References => _references;
    }

    [Serializable]
    public class ReferenceEntry
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

    [SerializeField] private List<ObjectEntry> _objects = new();

    public IReadOnlyList<ObjectEntry> Objects => _objects;
}
