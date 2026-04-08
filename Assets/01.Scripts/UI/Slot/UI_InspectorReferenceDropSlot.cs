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

    [SerializeField][Range(0f, 1f)] private float _idleHighlightAlpha = 0.2f;
    [SerializeField][Range(0f, 1f)] private float _hoverHighlightAlpha = 0.5f;

    [SerializeField] private Color _normalLabelColor = Color.white;
    [SerializeField] private Color _missingLabelColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color _wrongLabelColor = new Color(1f, 0.65f, 0.2f, 1f);

    private string _ownerId;
    private InspectorComponent _inspectorComponent;
    private string _slotId;

    public event Action<string, InspectorComponent, string, string> Dropped;

    public void Bind(string ownerId, InspectorComponent inspectorComponent, UI_RuntimeReferenceData referenceData)
    {
        _ownerId = ownerId;
        _inspectorComponent = inspectorComponent;
        _slotId = referenceData != null ? referenceData.SlotId : null;

        SetHighlightAlpha(_idleHighlightAlpha);

        if (referenceData == null)
        {
            if (_labelText != null)
                _labelText.text = string.Empty;

            if (_valueText != null)
                _valueText.text = "Empty";

            ApplyLabelColor(UI_ReferenceValidationErrorType.None);
            return;
        }

        if (_labelText != null)
            _labelText.text = referenceData.Label;

        if (_valueText != null)
            _valueText.text = string.IsNullOrEmpty(referenceData.CurrentTargetDisplayName)
                ? "Empty"
                : referenceData.CurrentTargetDisplayName;

        ApplyLabelColor(referenceData.GetErrorType());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UI_DragContext.IsDragging)
            SetHighlightAlpha(_hoverHighlightAlpha);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlightAlpha(_idleHighlightAlpha);
    }

    public void OnDrop(PointerEventData eventData)
    {
        SetHighlightAlpha(_idleHighlightAlpha);

        if (UI_DragContext.IsDragging == false)
            return;

        if (string.IsNullOrEmpty(_ownerId) || string.IsNullOrEmpty(_slotId))
            return;

        if (string.IsNullOrEmpty(UI_DragContext.DraggedComponentId))
            return;

        Dropped?.Invoke(_ownerId, _inspectorComponent, _slotId, UI_DragContext.DraggedComponentId);
    }

    private void ApplyLabelColor(UI_ReferenceValidationErrorType errorType)
    {
        if (_labelText == null)
            return;

        switch (errorType)
        {
            case UI_ReferenceValidationErrorType.MissingReference:
                _labelText.color = _missingLabelColor;
                break;

            case UI_ReferenceValidationErrorType.WrongReference:
                _labelText.color = _wrongLabelColor;
                break;

            default:
                _labelText.color = _normalLabelColor;
                break;
        }
    }

    private void SetHighlightAlpha(float alpha)
    {
        if (_highlightImage == null)
            return;

        _highlightImage.enabled = true;

        var color = _highlightImage.color;
        color.a = alpha;
        _highlightImage.color = color;
    }
}
