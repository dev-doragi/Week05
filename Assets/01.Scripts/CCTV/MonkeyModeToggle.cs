using UnityEngine;

public class MonkeyModeToggle : MonoBehaviour
{
    public void OnToggleChanged(bool isOn)
    {
        if (CameraManager.Instance != null)
            CameraManager.Instance.SetHardMode(isOn);
    }
}