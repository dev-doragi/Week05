using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_HierarchyPresenter : MonoBehaviour
{
    [SerializeField] private List<UI_HierarchyItemSlot> _hierarchyItemSlots;

    public event Action<string> ItemClicked;

    private void Awake()
    {
        foreach (var slot in _hierarchyItemSlots)
        {
            if (slot == null)
                continue;

            slot.Clicked += HandleItemClicked;
        }
    }

    private void OnDestroy()
    {
        foreach (var slot in _hierarchyItemSlots)
        {
            if (slot == null)
                continue;

            slot.Clicked -= HandleItemClicked;
        }
    }

    public void Render(IReadOnlyList<UI_InGameEditorRuntimeData> items, string selectedComponentId)
    {
        RefreshSlots(items.Count);
        RefreshViews(items, selectedComponentId);
    }

    private void RefreshSlots(int count)
    {
        for (int i = 0; i < _hierarchyItemSlots.Count; i++)
        {
            bool shouldShow = i < count;
            _hierarchyItemSlots[i].gameObject.SetActive(shouldShow);
        }

        if (count > _hierarchyItemSlots.Count)
            Debug.LogWarning("UI_HierarchyPresenter: 슬롯 개수가 부족합니다.");
    }

    private void RefreshViews(IReadOnlyList<UI_InGameEditorRuntimeData> items, string selectedComponentId)
    {
        int count = Mathf.Min(items.Count, _hierarchyItemSlots.Count);

        for (int i = 0; i < count; i++)
        {
            bool isSelected = items[i].Id == selectedComponentId;
            bool hasReferenceError = items[i].HasReferenceError();

            _hierarchyItemSlots[i].Bind(items[i], isSelected, hasReferenceError);
        }
    }

    private void HandleItemClicked(string componentId)
    {
        ItemClicked?.Invoke(componentId);
    }
}
