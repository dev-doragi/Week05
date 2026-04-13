using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static event Action OnGameStart;

    public bool GameFinish = false;

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
    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void GameOver()
    {
        if (GameFinish) return;

        if (StageManager.Instance != null)
        {
            var uncleared = StageManager.Instance.GetUnclearedStageDefinitionsSnapshot();
            EndingRuntimePayload.SetFailedStages(uncleared);
        }

        SceneManager.LoadScene("02.GameOverScene");
        GameFinish = true;
    }

    public void GameClear()
    {
        if (GameFinish) return;

        if (StageManager.Instance != null)
        {
            var cleared = StageManager.Instance.GetClearedStageDefinitionsSnapshot();
            EndingRuntimePayload.SetClearedStages(cleared);
        }

        SceneManager.LoadScene("03.GameClearScene");
        GameFinish = true;
    }



}