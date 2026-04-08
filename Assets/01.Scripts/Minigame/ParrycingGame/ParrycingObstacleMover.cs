using UnityEngine;
using System;

public class ParrycingObstacleMover : MonoBehaviour
{
    public float speed = 3f;
    public Transform spawnTop;
    public Transform spawnBottom;
    public Transform player;

    public static event Action OnDodged;
    public static event Action OnHitPlayer;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.transform == player)
            OnHitPlayer?.Invoke();
        else
            OnDodged?.Invoke();

        gameObject.SetActive(false);
    }

    public void ResetPosition()
    {
        bool goTop = UnityEngine.Random.value > 0.5f;
        transform.position = goTop ? spawnTop.position : spawnBottom.position;
    }
}