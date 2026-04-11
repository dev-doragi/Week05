using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static event Action OnGameStart;


    protected override void Init()
    {
        
    }

    public void StartGame()
    {
        OnGameStart?.Invoke();
        StageManager.Instance.StartStages();
        //GameFlowManager.Instance?.BeginFlow();
        
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}