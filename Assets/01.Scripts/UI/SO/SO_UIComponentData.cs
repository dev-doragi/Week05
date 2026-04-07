using UnityEngine;

[CreateAssetMenu(fileName = "SO_UIComponentData", menuName = "Scriptable Objects/SO_UIComponentData")]
public class SO_ComponentData : ScriptableObject
{
    /// <summary>
    /// 모든 에디터 창에서 사용하는 데이터. 이미지는 사용 안할수도있긴함
    /// </summary>
    [Header("DisplayData")]
    [SerializeField] private string _componentName;
    [SerializeField] private string _imageId;

    /// <summary>
    /// 일치 여부 등을 체크하기 위한 것
    /// </summary>
    [Header("ComponentData")]
    [SerializeField] private int _componentId;
    [SerializeField] private ComponentType _componentType;
}

public enum ComponentType
{
    Prefab,
    Sprite,
    Script,
    Console,
    Camera,
    Light,
}