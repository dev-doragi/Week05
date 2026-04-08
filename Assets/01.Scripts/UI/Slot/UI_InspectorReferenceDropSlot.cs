using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InspectorReferenceDropSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text _labelText;
    [SerializeField] private TMP_Text _valueText;
    [SerializeField] private Image _highlightImage;

    private string _ownerId;
    private string _slotId;

    public event Action<string, string, string> Dropped;

    public void Bind(string ownerId, UI_RuntimeReferenceData referenceData)
    {
        _ownerId = ownerId;
        _slotId = referenceData != null ? referenceData.SlotId : null;

        if (_highlightImage != null)
            _highlightImage.enabled = false;

        if (referenceData == null)
        {
            if (_labelText != null)
                _labelText.text = string.Empty;

            if (_valueText != null)
                _valueText.text = "Empty";

            return;
        }

        if (_labelText != null)
            _labelText.text = referenceData.Label;

        if (_valueText != null)
            _valueText.text = string.IsNullOrEmpty(referenceData.CurrentTargetId)
                ? "Empty"
                : referenceData.CurrentTargetId;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_highlightImage != null && UI_DragContext.IsDragging)
            _highlightImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_highlightImage != null)
            _highlightImage.enabled = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (_highlightImage != null)
            _highlightImage.enabled = false;

        if (UI_DragContext.IsDragging == false)
            return;

        if (string.IsNullOrEmpty(_ownerId) || string.IsNullOrEmpty(_slotId))
            return;

        if (string.IsNullOrEmpty(UI_DragContext.DraggedComponentId))
            return;

        Dropped?.Invoke(_ownerId, _slotId, UI_DragContext.DraggedComponentId);
    }
}
