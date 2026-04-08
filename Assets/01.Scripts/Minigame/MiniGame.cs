using UnityEngine;
using System;

public abstract class MiniGame : MonoBehaviour
{
    public static event Action<MiniGame> OnCleared;
    [SerializeField] private IssueDefinition issue;
    public IssueDefinition Issue => issue;
    public void StartGame()
    {
        gameObject.SetActive(true);
        OnStart();
    }

    protected void Clear()
    {
        OnCleared?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected abstract void OnStart();
    public void CompleteFromChild()
    {
        Clear();
    }
}