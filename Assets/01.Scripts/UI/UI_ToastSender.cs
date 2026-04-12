using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UI_ToastSender : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_ToastItem _toastItemPrefab;
    [SerializeField] private RectTransform _toastRoot;

    [Header("Layout")]
    [SerializeField] private float _stackSpacing = 20f;
    [SerializeField] private float _moveDuration = 0.25f;

    private readonly List<UI_ToastItem> _activeToastItems = new List<UI_ToastItem>();

    private void Awake()
    {
        if (_toastRoot == null)
            _toastRoot = transform as RectTransform;
    }

    public void Show(SO_ToastData data)
    {
        if (data == null || _toastItemPrefab == null || _toastRoot == null)
            return;

        UI_ToastItem toastItem = Instantiate(_toastItemPrefab, _toastRoot);
        toastItem.Initialize(this, data);

        _activeToastItems.Add(toastItem);
        RefreshToastPositions();
    }

    public void NotifyToastExpired(UI_ToastItem toastItem)
    {
        if (toastItem == null)
            return;

        if (_activeToastItems.Remove(toastItem) == false)
            return;

        RefreshToastPositions();
    }

    private void RefreshToastPositions()
    {
        for (int i = 0; i < _activeToastItems.Count; i++)
        {
            UI_ToastItem toastItem = _activeToastItems[i];
            if (toastItem == null)
                continue;

            float targetY = toastItem.BaseAnchoredY + i * (toastItem.Height + _stackSpacing);
            toastItem.MoveToStackPosition(targetY, _moveDuration);
        }
    }
}