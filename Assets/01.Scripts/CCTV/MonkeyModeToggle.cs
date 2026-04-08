using UnityEngine;
using UnityEngine.UI;

public class MonkeyModeToggle : MonoBehaviour
{
    public void OnToggleChanged(bool isOn)
    {
        CameraAreaController.SetHardMode(isOn);
    }
}