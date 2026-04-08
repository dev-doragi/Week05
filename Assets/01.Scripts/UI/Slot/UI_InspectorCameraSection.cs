using System;
using TMPro;
using UnityEngine;

public class UI_InspectorCameraSection : MonoBehaviour, IUIInspectorSection
{
    [SerializeField] private TMP_Text _titleText;

    public event Action<string, string, string> ReferenceDropped;

    public void Bind(string ownerId, UI_RuntimeInspectorSectionData sectionData)
    {
        if (_titleText != null)
            _titleText.text = "Camera";
    }
}
