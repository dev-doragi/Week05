using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public static event Action OnGameStart;

    [SerializeField] private Button _gameStartButton;

    protected override void Init()
    {
        if (_gameStartButton != null)
        {
            _gameStartButton.onClick.AddListener(StartGame);
        }
    }

    public void StartGame()
    {
        OnGameStart?.Invoke();

        GameFlowManager.Instance?.BeginFlow();

        if (_gameStartButton != null)
            _gameStartButton.gameObject.SetActive(false);
    }
}