using TMPro;
using UnityEngine;

public class UI_InspectorEnemyControllerScriptSection : MonoBehaviour, IUIInspectorSection
{
    [SerializeField] private TMP_Text _titleText;

    public void Bind(string ownerId, UI_RuntimeInspectorSectionData sectionData)
    {
        if (_titleText != null)
            _titleText.text = "Enemy Controller";
    }
}
