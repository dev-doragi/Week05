using UnityEngine;
using System;

public class BombTalpinoTarget : MonoBehaviour
{
    public static event Action OnDestroyed;

    public void GetHit()
    {
        OnDestroyed?.Invoke();
        gameObject.SetActive(false);
    }
}