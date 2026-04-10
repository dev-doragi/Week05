using UnityEngine;

public class UI_NullRefTest : MonoBehaviour
{
    [SerializeField] private UI_RuntimeReferenceBlocker _blocker;
    private bool _isBlocked;

    private void OnEnable()
    {
        _blocker.BlockedChanged += HandleBlockedChanged;
        HandleBlockedChanged(_blocker.IsBlocked);
    }

    private void OnDisable()
    {
        _blocker.BlockedChanged -= HandleBlockedChanged;
    }

    private void Update()
    {
        if (_isBlocked)
            return;
    }

    private void HandleBlockedChanged(bool isBlocked)
    {
        _isBlocked = isBlocked;
        Debug.Log(isBlocked ? "NullRefTest: 블락" : "NullRefTest: 언블락");
    }
}
