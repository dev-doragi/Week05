using UnityEngine;

public class GroundCheck2D : MonoBehaviour
{
    [SerializeField] private Transform checkPoint;
    [SerializeField] private float checkRadius = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded;

    private void Reset()
    {
        checkPoint = transform;
    }

    private void FixedUpdate()
    {
        if (checkPoint == null)
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = Physics2D.OverlapCircle(checkPoint.position, checkRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (checkPoint == null) return;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPoint.position, checkRadius);
    }
}
