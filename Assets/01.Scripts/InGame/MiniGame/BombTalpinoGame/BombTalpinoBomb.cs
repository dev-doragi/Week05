using UnityEngine;

public class BombTalpinoBomb : MonoBehaviour
{
    public float explosionRadius = 1.5f;

    Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Fire(Vector2 dir, float force)
    {
        gameObject.SetActive(true);
        _rb.linearVelocity = Vector2.zero;  // 이전 속도 리셋 필수
        _rb.angularVelocity = 0f;
        Debug.Log($"[BombTalpinoBomb] Fire: dir={dir}, force={force}");
        _rb.AddForce(dir * force, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Explode();
    }

    void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }

    void Explode()
    {
        // 폭발 범위 내 오브젝트 판별
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            BombTalpinoWall wall = hit.GetComponent<BombTalpinoWall>();
            BombTalpinoTarget target = hit.GetComponent<BombTalpinoTarget>();

            if (wall != null) wall.GetHit();
            if (target != null) target.GetHit();
        }

        gameObject.SetActive(false);
    }
}