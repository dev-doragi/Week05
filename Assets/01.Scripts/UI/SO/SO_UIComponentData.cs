using UnityEngine;

[CreateAssetMenu(fileName = "SO_UIComponentData", menuName = "Scriptable Objects/SO_UIComponentData")]
public class SO_ComponentData : ScriptableObject
{
    [Header("DisplayData")]
    [SerializeField] private string _componentName;
    [SerializeField] private string _imageId;

    [Header("ComponentData")]
    [SerializeField] private int _componentId;
    [SerializeField] private ComponentType _componentType;
}

public enum ComponentType
{
    Prefab,
    Image,
    Script,
    Console,
}