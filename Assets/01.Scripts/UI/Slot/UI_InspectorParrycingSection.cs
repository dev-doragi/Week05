using System;
using TMPro;
using UnityEngine;

public class UI_InspectorParrycingSection : MonoBehaviour, IUIInspectorReferenceSection
{
    [SerializeField] private TMP_Text _titleText;

    [SerializeField] private string _firstSlotId = "reference01";
    [SerializeField] private UI_InspectorReferenceDropSlot _firstReferenceSlot;

    [SerializeField] private string _secondSlotId = "reference02";
    [SerializeField] private UI_InspectorReferenceDropSlot _secondReferenceSlot;

    [SerializeField] private string _thirdSlotId = "reference03";
    [SerializeField] private UI_InspectorReferenceDropSlot _thirdReferenceSlot;

    [SerializeField] private string _fourthSlotId = "reference04";
    [SerializeField] private UI_InspectorReferenceDropSlot _fourthReferenceSlot;

    [SerializeField] private string _fifthSlotId = "reference05";
    [SerializeField] private UI_InspectorReferenceDropSlot _fifthReferenceSlot;

    public event Action<string, InspectorComponent, string, string> ReferenceDropped;

    private void Awake()
    {
        if (_firstReferenceSlot != null)
            _firstReferenceSlot.Dropped += HandleDropped;

        if (_secondReferenceSlot != null)
            _secondReferenceSlot.Dropped += HandleDropped;

        if (_thirdReferenceSlot != null)
            _thirdReferenceSlot.Dropped += HandleDropped;

        if (_fourthReferenceSlot != null)
            _fourthReferenceSlot.Dropped += HandleDropped;

        if (_fifthReferenceSlot != null)
            _fifthReferenceSlot.Dropped += HandleDropped;
    }

    private void OnDestroy()
    {
        if (_firstReferenceSlot != null)
            _firstReferenceSlot.Dropped -= HandleDropped;

        if (_secondReferenceSlot != null)
            _secondReferenceSlot.Dropped -= HandleDropped;

        if (_thirdReferenceSlot != null)
            _thirdReferenceSlot.Dropped -= HandleDropped;

        if (_fourthReferenceSlot != null)
            _fourthReferenceSlot.Dropped -= HandleDropped;

        if (_fifthReferenceSlot != null)
            _fifthReferenceSlot.Dropped -= HandleDropped;
    }

    public void Bind(string ownerId, UI_RuntimeInspectorSectionData sectionData)
    {
        if (_titleText != null)
            _titleText.text = "Parrycing";

        if (_firstReferenceSlot != null)
            _firstReferenceSlot.Bind(ownerId, sectionData.InspectorComponent, FindReference(sectionData, _firstSlotId));

        if (_secondReferenceSlot != null)
            _secondReferenceSlot.Bind(ownerId, sectionData.InspectorComponent, FindReference(sectionData, _secondSlotId));

        if (_thirdReferenceSlot != null)
            _thirdReferenceSlot.Bind(ownerId, sectionData.InspectorComponent, FindReference(sectionData, _thirdSlotId));

        if (_fourthReferenceSlot != null)
            _fourthReferenceSlot.Bind(ownerId, sectionData.InspectorComponent, FindReference(sectionData, _fourthSlotId));

        if (_fifthReferenceSlot != null)
            _fifthReferenceSlot.Bind(ownerId, sectionData.InspectorComponent, FindReference(sectionData, _fifthSlotId));
    }

    private UI_RuntimeReferenceData FindReference(UI_RuntimeInspectorSectionData sectionData, string slotId)
    {
        if (sectionData == null)
            return null;

        foreach (var referenceData in sectionData.References)
        {
            if (referenceData.SlotId == slotId)
                return referenceData;
        }

        return null;
    }

    private void HandleDropped(string ownerId, InspectorComponent inspectorComponent, string slotId, string targetId)
    {
        ReferenceDropped?.Invoke(ownerId, inspectorComponent, slotId, targetId);
    }
}
