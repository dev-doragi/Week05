using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_ComponentSelectable : MonoBehaviour
{
    [SerializeField] private GameObject _selectedHoverImage;
    [SerializeField] private Button _componentButton;

    private string _componentId;
    public event Action<string> Clicked;

    public void Bind(string componentId)
    {
        _componentId = componentId;
    }

    private void OnEnable()
    {
        _componentButton.onClick.AddListener(HandleClick);
    }

    private void OnDisable()
    {
        _componentButton.onClick.RemoveListener(HandleClick);
    }

    private void HandleClick()
    {
        Clicked?.Invoke(_componentId);
    }

    public void SetSelected(bool isSelected)
    {
        _selectedHoverImage.SetActive(isSelected);
    }
}
