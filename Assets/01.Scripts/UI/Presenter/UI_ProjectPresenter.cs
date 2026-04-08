using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_ProjectPresenter : MonoBehaviour
{
    [SerializeField] private List<UI_ProjectItemSlot> _projectItemSlots;

    public event Action<string> ItemClicked;

    private void Awake()
    {
        foreach (var slot in _projectItemSlots)
        {
            if (slot == null)
                continue;

            slot.Clicked += HandleItemClicked;
        }
    }

    private void OnDestroy()
    {
        foreach (var slot in _projectItemSlots)
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
        for (int i = 0; i < _projectItemSlots.Count; i++)
        {
            bool shouldShow = i < count;
            _projectItemSlots[i].gameObject.SetActive(shouldShow);
        }

        if (count > _projectItemSlots.Count)
            Debug.LogWarning("UI_ProjectPresenter: 슬롯 개수가 부족합니다.");
    }

    private void RefreshViews(IReadOnlyList<UI_InGameEditorRuntimeData> items, string selectedComponentId)
    {
        int count = Mathf.Min(items.Count, _projectItemSlots.Count);

        for (int i = 0; i < count; i++)
        {
            bool isSelected = items[i].Id == selectedComponentId;
            _projectItemSlots[i].Bind(items[i], isSelected);
        }
    }

    private void HandleItemClicked(string componentId)
    {
        ItemClicked?.Invoke(componentId);
    }
}
