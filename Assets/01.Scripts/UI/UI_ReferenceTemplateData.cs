using UnityEngine;

[System.Serializable]
public class UI_ReferenceTemplateData
{
    [SerializeField] private InspectorComponent _inspectorComponent;
    [SerializeField] private string _slotId;
    [SerializeField] private string _label;
    [SerializeField] private string _defaultTargetId;
    [SerializeField] private bool _canSpawnError = true;
    [SerializeField] private bool _isRequired = true;

    public InspectorComponent InspectorComponent => _inspectorComponent;
    public string SlotId => _slotId;
    public string Label => _label;
    public string DefaultTargetId => _defaultTargetId;
    public bool CanSpawnError => _canSpawnError;
    public bool IsRequired => _isRequired;
}
