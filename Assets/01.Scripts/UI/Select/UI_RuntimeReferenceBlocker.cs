using System;
using UnityEngine;

public class UI_RuntimeReferenceBlocker : MonoBehaviour
{
    [SerializeField] private GameObjectSelectable _selectable;

    private UI_InGameEditorRuntimeState _runtimeState;

    public bool IsBlocked { get; private set; }
    public event Action<bool> BlockedChanged;

    private void Awake()
    {
        _selectable ??= GetComponent<GameObjectSelectable>();

        if (_selectable != null)
            _selectable.ComponentIdChanged += HandleComponentIdChanged;
    }

    private void Start()
    {
        BindRuntimeState();
        RefreshBlockedState();
    }

    private void OnEnable()
    {
        BindRuntimeState();
        RefreshBlockedState();
    }

    private void OnDisable()
    {
        UnbindRuntimeState();
    }

    private void OnDestroy()
    {
        if (_selectable != null)
            _selectable.ComponentIdChanged -= HandleComponentIdChanged;
    }

    public string OwnerId => _selectable != null ? _selectable.ComponentId : null;

    public void RefreshBlockedState()
    {
        BindRuntimeState();

        bool isBlocked =
            _runtimeState != null &&
            string.IsNullOrEmpty(OwnerId) == false &&
            _runtimeState.IsOwnerBlocked(OwnerId);

        SetBlocked(isBlocked);
    }

    private void BindRuntimeState()
    {
        var nextState = InGameEditorController.EditorRuntimeState;
        if (ReferenceEquals(_runtimeState, nextState))
            return;

        UnbindRuntimeState();
        _runtimeState = nextState;

        if (_runtimeState != null)
            _runtimeState.OwnerBlockStateChanged += HandleOwnerBlockStateChanged;
    }

    private void UnbindRuntimeState()
    {
        if (_runtimeState != null)
            _runtimeState.OwnerBlockStateChanged -= HandleOwnerBlockStateChanged;

        _runtimeState = null;
    }

    private void HandleComponentIdChanged(string _)
    {
        RefreshBlockedState();
    }

    private void HandleOwnerBlockStateChanged(string ownerId, bool isBlocked)
    {
        if (ownerId != OwnerId)
            return;

        SetBlocked(isBlocked);
    }

    private void SetBlocked(bool isBlocked)
    {
        if (IsBlocked == isBlocked)
            return;

        IsBlocked = isBlocked;
        BlockedChanged?.Invoke(IsBlocked);
    }
}
