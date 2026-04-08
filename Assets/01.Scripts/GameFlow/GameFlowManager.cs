using System.Collections;
using UnityEngine;

public class GameFlowManager : Singleton<GameFlowManager>
{
    public enum FlowState
    {
        Ready,
        Standby,
        Debug,
        Ingame,
        Clear
    }

    [Header("Refs")]
    [SerializeField] private ETypePoolManager poolManager;
    [SerializeField] private GameObject game1DebugRoot;
    [SerializeField] private GameObject game1IngameRoot;

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

    protected override void Init()
    {
        State = FlowState.Ready;
        SetMiniGamesActive(false, false);
    }

    // Build 버튼 시점에 호출
    public void BeginFlow()
    {
        if (poolManager == null)
        {
            Debug.LogError("[GameFlow] poolManager is null.");
            return;
        }

        StopFlowInternal();

        poolManager.ResetPool();
        flowRunning = true;
        waitingChoice = false;
        currentIssue = null;
        SetMiniGamesActive(false, false);

        EnterStandby();
    }

    // Debug 미니게임 클리어 트리거
    public void NotifyDebugCleared()
    {
        if (!flowRunning || State != FlowState.Debug) return;
        if (game1DebugRoot != null && game1DebugRoot.activeSelf) game1DebugRoot.SetActive(false);

        waitingChoice = true; 
        UIManager.Instance.InGameChoiceButtonActive(true);

    }


    public void ResolveDebugChoice(bool playIngame)
    {
        if (!flowRunning || State != FlowState.Debug || !waitingChoice) return;

        waitingChoice = false;
        UIManager.Instance.InGameChoiceButtonActive(false);
        UIManager.Instance.DebugGamePannelActive(false);


        if (playIngame)
        {
            State = FlowState.Ingame;
            if (game1IngameRoot != null) game1IngameRoot.SetActive(true);
            Debug.Log("[GameFlow] Choice=Play -> Ingame");
        }
        else
        {
            bool returned = poolManager.ReturnIssueWithChance(currentIssue, skipReturnChance);

            EndTurn();
        }
    }

    // Ingame 미니게임 클리어 트리거
    public void NotifyIngameCleared()
    {
        if (!flowRunning || State != FlowState.Ingame) return;
        if (game1IngameRoot != null && game1IngameRoot.activeSelf) game1IngameRoot.SetActive(false);

        EndTurn();
    }

    public void GameClear()
    {
        StopFlowInternal();
        State = FlowState.Clear;
        waitingChoice = false;
        currentIssue = null;
        SetMiniGamesActive(false, false);
    }

    private void EnterStandby()
    {
        State = FlowState.Standby;
        waitingChoice = false;
        SetMiniGamesActive(false, false);

        if (standbyRoutine != null) StopCoroutine(standbyRoutine);
        standbyRoutine = StartCoroutine(CoStandbyThenDraw());
    }

    private IEnumerator CoStandbyThenDraw()
    {

        if (!flowRunning) yield break;
        
        yield return new WaitForSeconds(standbySeconds);
        if (!poolManager.TryDrawRandomIssue(out currentIssue))
        {
            GameClear();
            yield break;
        }
        State = FlowState.Debug;
        if (game1DebugRoot != null) game1DebugRoot.SetActive(true);
        UIManager.Instance.DebugGamePannelActive(true);


        Debug.Log($"Debug Game Start: {currentIssue.IssueId}");
    }

    private void EndTurn()
    {
        currentIssue = null;
        EnterStandby();
    }

    private void SetMiniGamesActive(bool debugActive, bool ingameActive)
    {
        if (game1DebugRoot != null) game1DebugRoot.SetActive(debugActive);
        if (game1IngameRoot != null) game1IngameRoot.SetActive(ingameActive);
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
