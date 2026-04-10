using UnityEngine;
using UnityEngine.InputSystem;

public class DentureCollectPlayer : MonoBehaviour
{
    public Transform leftBoundary;
    public Transform rightBoundary;

    public event System.Action<DentureCollectDenture> OnCaught;

    void Update()
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = -Camera.main.transform.position.z;
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(screenPos);

        float clampedX = Mathf.Clamp(mouseWorld.x, leftBoundary.position.x, rightBoundary.position.x);
        transform.position = new Vector3(clampedX, transform.position.y, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DentureCollectDenture denture = other.GetComponent<DentureCollectDenture>();
        if (denture == null) return;
        OnCaught?.Invoke(denture);
    }
}