using System.Collections;
using UnityEngine;

public enum FlowState
{
    Ready,
    Standby,
    Debug,
    Ingame,
    Clear
}

public class GameFlowManager : Singleton<GameFlowManager>
{
    [Header("Refs")]
    [SerializeField] private ETypePoolManager poolManager;
    public MiniGame[] DebugMiniGames;
    public MiniGame[] InGameMiniGames;

    [Header("Flow")]
    [SerializeField] private float standbySeconds = 10f;
    [SerializeField] private float skipReturnChance = 0.45f;

    public FlowState State { get; private set; } = FlowState.Ready;
    public bool IsWaitingChoice => waitingChoice;
    public IssueDefinition CurrentIssue => currentIssue;

    private bool flowRunning;
    private bool waitingChoice;
    private IssueDefinition currentIssue;
    private Coroutine standbyRoutine;

    private MiniGame activeDebugMiniGame;
    private MiniGame activeIngameMiniGame;

    protected override void Init()
    {
        State = FlowState.Ready;
        SetAllMiniGamesActive(false);
    }

    private void OnEnable()
    {
        MiniGame.OnCleared += HandleMiniGameCleared;
    }

    private void OnDisable()
    {
        MiniGame.OnCleared -= HandleMiniGameCleared;
    }

    private void HandleMiniGameCleared(MiniGame cleared)
    {
        if (!flowRunning || cleared == null) return;

        if (State == FlowState.Debug && IsInList(DebugMiniGames, cleared))
        {
            NotifyDebugCleared();
            return;
        }

        if (State == FlowState.Ingame && IsInList(InGameMiniGames, cleared))
        {
            UIManager.Instance.NoiseActive(false);
            NotifyIngameCleared();
        }
    }

    private bool IsInList(MiniGame[] list, MiniGame target)
    {
        if (list == null) return false;
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] == target) return true;
        }
        return false;
    }

    public void BeginFlow()
    {
        if (poolManager == null) return;

        StopFlowInternal();
        poolManager.ResetPool();

        flowRunning = true;
        waitingChoice = false;
        currentIssue = null;
        activeDebugMiniGame = null;
        activeIngameMiniGame = null;

        SetAllMiniGamesActive(false);
        UIManager.Instance.InGameChoiceButtonActive(false);
        UIManager.Instance.DebugGamePannelActive(false);

        EnterStandby();
    }

    public void NotifyDebugCleared()
    {
        if (!flowRunning || State != FlowState.Debug) return;

        waitingChoice = true;
        UIManager.Instance.InGameChoiceButtonActive(true);

        // 깜빡임 실행(StartBlink) 로직 삭제
    }

    public void ResolveDebugChoice(bool playIngame)
    {
        if (!flowRunning || State != FlowState.Debug || !waitingChoice) return;

        waitingChoice = false;

        // 깜빡임 중지(StopBlink) 로직 삭제

        UIManager.Instance.InGameChoiceButtonActive(false);
        UIManager.Instance.DebugGamePannelActive(false);

        if (playIngame)
        {
            State = FlowState.Ingame;
            activeIngameMiniGame = FindMiniGameByIssue(InGameMiniGames, currentIssue);

            if (activeIngameMiniGame == null)
            {
                EndTurn();
                return;
            }

            activeIngameMiniGame.StartGame();
        }
        else
        {
            UIManager.Instance.NoiseActive(false);
            poolManager.ReturnIssueWithChance(currentIssue, skipReturnChance);
            EndTurn();
        }
    }

    public void NotifyIngameCleared()
    {
        if (!flowRunning || State != FlowState.Ingame) return;
        EndTurn();
    }

    public void GameClear()
    {
        StopFlowInternal();
        State = FlowState.Clear;
        UIManager.Instance.NoiseActive(false);

        waitingChoice = false;
        currentIssue = null;
        activeDebugMiniGame = null;
        activeIngameMiniGame = null;

        SetAllMiniGamesActive(false);
        UIManager.Instance.InGameChoiceButtonActive(false);
        UIManager.Instance.DebugGamePannelActive(false);
    }

    private void EnterStandby()
    {
        State = FlowState.Standby;
        waitingChoice = false;
        SetAllMiniGamesActive(false);

        UIManager.Instance.InGameChoiceButtonActive(false);
        UIManager.Instance.DebugGamePannelActive(false);

        if (standbyRoutine != null) StopCoroutine(standbyRoutine);
        standbyRoutine = StartCoroutine(StandbyMiniGame());
    }

    private IEnumerator StandbyMiniGame()
    {
        if (!flowRunning) yield break;

        yield return new WaitForSeconds(standbySeconds);

        if (!poolManager.TryDrawRandomIssue(out currentIssue))
        {
            GameClear();
            yield break;
        }

        State = FlowState.Debug;
        activeDebugMiniGame = FindMiniGameByIssue(DebugMiniGames, currentIssue);

        if (activeDebugMiniGame == null)
        {
            EndTurn();
            yield break;
        }
        UIManager.Instance.LogMessageActive(true);
        LogManager.Instance.UpdateIssueLog(currentIssue);

        UIManager.Instance.SetUnityAlert(true);
        UIManager.Instance.NoiseActive(true);

        UIManager.Instance.DebugGamePannelActive(true);
        activeDebugMiniGame.StartGame();
    }

    private void EndTurn()
    {
        currentIssue = null;
        activeDebugMiniGame = null;
        activeIngameMiniGame = null;
        EnterStandby();
    }

    private MiniGame FindMiniGameByIssue(MiniGame[] list, IssueDefinition issue)
    {
        if (list == null || issue == null) return null;

        for (int i = 0; i < list.Length; i++)
        {
            MiniGame mg = list[i];
            if (mg != null && mg.Issue == issue)
                return mg;
        }

        return null;
    }

    private void SetAllMiniGamesActive(bool active)
    {
        SetArrayActive(DebugMiniGames, active);
        SetArrayActive(InGameMiniGames, active);
    }

    private void SetArrayActive(MiniGame[] list, bool active)
    {
        if (list == null) return;

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
                list[i].gameObject.SetActive(active);
        }
    }

    private void StopFlowInternal()
    {
        flowRunning = false;

        if (standbyRoutine != null)
        {
            StopCoroutine(standbyRoutine);
            standbyRoutine = null;
        }
    }
}