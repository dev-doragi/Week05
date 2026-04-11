using UnityEngine;
using System;

public class CollisionReporter : MonoBehaviour
{
    public static event Action<Collider2D> OnEnter2D;

    void OnCollisionEnter2D(Collision2D col)
    {
        OnEnter2D?.Invoke(col.collider);
    }
}