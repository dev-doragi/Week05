using UnityEngine;
using System;

public abstract class MiniGame : MonoBehaviour
{
    public static event Action<MiniGame> OnCleared;

    public void StartGame()
    {
        gameObject.SetActive(true);
        OnStart();
    }

    protected void Clear()
    {
        Debug.Log($"[MiniGame] {gameObject.name} CLEAR");
        OnCleared?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected abstract void OnStart();
}