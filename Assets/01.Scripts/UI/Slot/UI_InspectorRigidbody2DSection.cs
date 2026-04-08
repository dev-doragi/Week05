using TMPro;
using UnityEngine;

public class UI_InspectorRigidbody2DSection : MonoBehaviour, IUIInspectorSection
{
    [SerializeField] private TMP_Text _titleText;

    public void Bind(string ownerId, UI_RuntimeInspectorSectionData sectionData)
    {
        if (_titleText != null)
            _titleText.text = "Rigidbody 2D";
    }
}
