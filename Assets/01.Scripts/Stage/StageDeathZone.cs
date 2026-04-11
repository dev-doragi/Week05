using UnityEngine;

public class StageDeathZone : MonoBehaviour
{
    public Transform RespawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.GetComponent<PlayerMovement>() == null) return;

    other.transform.position = RespawnPoint.position;

    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
    if (rb != null)
        rb.linearVelocity = Vector2.zero;
}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlayerMovement>() == null) return;
        collision.collider.transform.position = RespawnPoint.position;
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

    }
}