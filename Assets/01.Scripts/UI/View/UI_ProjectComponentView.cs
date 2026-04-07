using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ProjectComponentView : MonoBehaviour
{
    [SerializeField] private TMP_Text _componentNameText;
    [SerializeField] private Image _iconImage;

    public void Render(string name, Image image)
    {
        _componentNameText.text = name;
        _iconImage = image;
    }
}
