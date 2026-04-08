using UnityEngine;
using System;

public class DentureCollectFloor : MonoBehaviour
{
    public static event Action OnMissed;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DentureCollectDenture>() == null) return;
        OnMissed?.Invoke();
        Destroy(other.gameObject);
    }
}