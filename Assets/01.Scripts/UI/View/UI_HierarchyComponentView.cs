using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_HierarchyComponentView : MonoBehaviour
{
    [SerializeField] private TMP_Text _componentNameText;
    [SerializeField] private Image _iconImage;

    public void Render(string name, Sprite iconSprite)
    {
        _componentNameText.text = name;
        _iconImage.sprite = iconSprite;
    }
}
