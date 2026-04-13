using UnityEngine;

public class MinimapRotationToggleButton : MonoBehaviour
{
    [SerializeField] private RoomSelectionGroup roomSelectionGroup;
    [SerializeField] private GameObject checkMarkImage;
    [SerializeField] private bool startEnabled = true;
    private bool _isEnabled;

    private void Awake()
    {
        ApplyState(startEnabled, true);
    }

    public void OnClickToggle()
    {
        ApplyState(!_isEnabled, false);
    }

    private void ApplyState(bool enabled, bool force)
    {
        if (!force && _isEnabled == enabled) return;

        _isEnabled = enabled;

        if (checkMarkImage != null)
            checkMarkImage.SetActive(_isEnabled);

        if (roomSelectionGroup != null)
            roomSelectionGroup.SetMapRotationEnabled(_isEnabled);
    }
}
