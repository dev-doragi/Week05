using UnityEngine;

public class BillBoard : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private bool flipY180 = true;

    private void LateUpdate()
    {
        if (minimapCamera == null) return;

        transform.rotation = minimapCamera.transform.rotation;

        if (flipY180)
            transform.Rotate(0f, 180f, 0f, Space.Self);
    }
}
