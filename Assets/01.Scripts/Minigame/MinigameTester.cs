using UnityEngine;

public class MinigameTester : MonoBehaviour
{
    [Header("MiniGames")]
    public MiniGame[] miniGames;

    void Start() => MiniGame.OnCleared += OnMiniGameCleared;
    void OnDestroy() => MiniGame.OnCleared -= OnMiniGameCleared;

    [ContextMenu("Play MiniGame 0")]
    void PlayMiniGame0() => PlayAt(0);

    [ContextMenu("Play MiniGame 1")]
    void PlayMiniGame1() => PlayAt(1);

    [ContextMenu("Play MiniGame 2")]
    void PlayMiniGame2() => PlayAt(2);

    void PlayAt(int index)
    {
        if (index >= miniGames.Length)
        {
            Debug.LogWarning($"[GameManager] index {index} 없음");
            return;
        }
        miniGames[index].StartGame();
    }

    void OnMiniGameCleared(MiniGame game)
    {
        Debug.Log($"[GameManager] {game.gameObject.name} 클리어!");
    }
}
