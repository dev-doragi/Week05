using UnityEngine;
using System;

public class JellyShotgunBullet : MonoBehaviour
{
    public float speed = 10f;

    public static event Action<GameObject> OnHitEnemy;

    Vector2 _dir;

    public void Fire(Vector2 dir)
    {
        _dir = dir.normalized;
        gameObject.SetActive(true);
    }

    void Update()
    {
        transform.Translate(_dir * speed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<JellyShotgunEnemyMover>() != null)
            OnHitEnemy?.Invoke(col.gameObject);

        gameObject.SetActive(false);
    }

    void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}