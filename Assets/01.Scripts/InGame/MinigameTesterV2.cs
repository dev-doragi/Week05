using System.Linq;
using UnityEngine;

public class MinigameTesterV2 : MonoBehaviour
{
    [Header("MiniGames")]
    public MiniGame[] debugGames;
    public MiniGame[] testGames;

    [Header("Flow Settings")]
    [Tooltip("SO 매핑 사용 여부(랜덤 여부)")]
    public bool useIssueSO = false;
    public IssueDefinition[] issues;

    private IssueDefinition _currentActiveIssue;
    [SerializeField] private bool _isNextDebug = true;
    

    void OnDestroy() => MiniGame.OnCleared -= OnMiniGameCleared;


    void Start()
    {
        MiniGame.OnCleared += OnMiniGameCleared;

        foreach (var g in debugGames) g.gameObject.SetActive(false);
        foreach (var g in testGames) g.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        MiniGame.OnCleared -= OnMiniGameCleared;
    }

    [ContextMenu("Start")]
    public void StartMiniGame()
    {
        _isNextDebug = true;
        PlayNext();
    }

    void PlayNext()
    {
        MiniGame nextGame = null;
        string logName = "None";

        // SO 매핑 (순서대로)
        if (useIssueSO)
        {
            if (issues == null || issues.Length == 0) return;

            // Debug Game
            if (_isNextDebug)
            {
                _currentActiveIssue = issues[Random.Range(0, issues.Length)];

                nextGame = FindGameByIssueId(debugGames, _currentActiveIssue.IssueId);
            }
            // Test Game
            else
            {
                _currentActiveIssue = issues[Random.Range(0, issues.Length)];

                nextGame = FindGameByIssueId(testGames, _currentActiveIssue.IssueId);
            }

            logName = _currentActiveIssue.IssueTitle;
        }
        // 랜덤
        else
        {
            if (_isNextDebug) nextGame = debugGames[Random.Range(0, debugGames.Length)];
            else nextGame = testGames[Random.Range(0, testGames.Length)];

            logName = nextGame.gameObject.name;
        }

        Debug.Log($"[TesterV2] {(_isNextDebug ? "Debug" : "Test")} Selected: {logName}");
        nextGame.StartGame();
    }

    void OnMiniGameCleared(MiniGame game)
    {
        Debug.Log($"[GameManager] {game.gameObject.name} 클리어!");

        _isNextDebug = !_isNextDebug;

        Invoke(nameof(PlayNext), 0.5f);
    }

    private MiniGame FindGameByIssueId(MiniGame[] array, string targetId)
    {
        if (array == null || string.IsNullOrEmpty(targetId)) return null;

        return array.FirstOrDefault(g =>
            g != null &&
            g.Issue != null &&
            g.Issue.IssueId == targetId
        );
    }

    [ContextMenu("Force All Stop")]
    void StopAll()
    {
        CancelInvoke();

        foreach (var g in debugGames) g.gameObject.SetActive(false);
        foreach (var g in testGames) g.gameObject.SetActive(false);
    }
}