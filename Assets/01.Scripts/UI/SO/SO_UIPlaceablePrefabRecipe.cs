using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_UIPlaceablePrefabRecipe", menuName = "Scriptable Objects/SO_UIPlaceablePrefabRecipe")]
public class SO_UIPlaceablePrefabRecipe : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private SO_ComponentData _componentData;

    [Header("Placement")]
    [SerializeField] private GameObject _placementPrefab;
    [SerializeField] private UIEditorScene _defaultScene = UIEditorScene.None;

    [Header("Default References")]
    [SerializeField] private List<UI_ReferenceTemplateData> _defaultReferences = new();

    public SO_ComponentData ComponentData => _componentData;
    public GameObject PlacementPrefab => _placementPrefab;
    public UIEditorScene DefaultScene => _defaultScene;
    public IReadOnlyList<UI_ReferenceTemplateData> DefaultReferences => _defaultReferences;
}
