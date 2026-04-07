using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsoleComponentView : MonoBehaviour
{
    [SerializeField] private Image _consoleLogImage;
    [SerializeField] private TMP_Text _consoleTitleText;
    [SerializeField] private TMP_Text _consoleContextText;


    public void Render(Image logImage, string title, string context)
    {
        _consoleLogImage = logImage;
        _consoleTitleText.text = title;
        _consoleContextText.text = context;
    }
}

