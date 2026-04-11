// using System.Collections;
// using UnityEngine;
// using System.Collections.Generic;

// public enum FlowState
// {
//     Ready,
//     Standby,
//     Debug,
//     Ingame,
//     Clear
// }

// public class GameFlowManager : Singleton<GameFlowManager>
// {
//     [Header("Refs")]
//     [SerializeField] private ETypePoolManager poolManager;
//     public MiniGame[] DebugMiniGames;
//     public MiniGame[] InGameMiniGames;

//     // UI_ButtonHover로 타입 변경 및 참조 복구
//     [Header("UI Feedback")]
//     [SerializeField] private UI_ButtonHover[] choiceButtons;

//     [Header("Flow")]
//     [SerializeField] private float standbySeconds = 10f;
//     [SerializeField] private float skipReturnChance = 0.45f;
//     [SerializeField] private ClearEndingController clearEndingController;

//     [SerializeField] private int clearGoalCount = 5; // 5개 깨면 클리어
//     [SerializeField] private int clearedCountInspector; // 디버그용
//     private int clearedCount;

//     private readonly List<IssueDefinition> appearedIssues = new List<IssueDefinition>();
//     private readonly HashSet<string> appearedKeys = new HashSet<string>();

//     private readonly List<IssueDefinition> solvedIssues = new List<IssueDefinition>();
//     private readonly HashSet<string> solvedKeys = new HashSet<string>();

//     public FlowState State { get; private set; } = FlowState.Ready;
//     public bool IsWaitingChoice => waitingChoice;
//     public IssueDefinition CurrentIssue => currentIssue;

//     private bool flowRunning;
//     private bool waitingChoice;
//     private IssueDefinition currentIssue;
//     private Coroutine standbyRoutine;

//     private MiniGame activeDebugMiniGame;
//     private MiniGame activeIngameMiniGame;

//     protected override void Init()
//     {
//         State = FlowState.Ready;
//         SetAllMiniGamesActive(false);
//     }

//     private void OnEnable()
//     {
//         MiniGame.OnCleared += HandleMiniGameCleared;
//     }

//     private void OnDisable()
//     {
//         MiniGame.OnCleared -= HandleMiniGameCleared;
//     }

//     private void HandleMiniGameCleared(MiniGame cleared)
//     {
//         if (!flowRunning || cleared == null) return;

//         if (State == FlowState.Debug && IsInList(DebugMiniGames, cleared))
//         {
//             NotifyDebugCleared();
//             return;
//         }

//         if (State == FlowState.Ingame && IsInList(InGameMiniGames, cleared))
//         {
//             UIManager.Instance.NoiseActive(false);
//             UIManager.Instance.SetCCTVAlert(true);
//             NotifyIngameCleared();
//         }
//     }

//     private bool IsInList(MiniGame[] list, MiniGame target)
//     {
//         if (list == null) return false;
//         for (int i = 0; i < list.Length; i++)
//         {
//             if (list[i] == target) return true;
//         }
//         return false;
//     }

//     public void BeginFlow()
//     {
//         appearedIssues.Clear();
//         appearedKeys.Clear();
//         solvedIssues.Clear();
//         solvedKeys.Clear();
//         if (poolManager == null) return;

//         StopFlowInternal();
//         poolManager.ResetPool();

//         flowRunning = true;
//         waitingChoice = false;
//         currentIssue = null;
//         activeDebugMiniGame = null;
//         activeIngameMiniGame = null;

//         clearedCount = 0;
//         clearedCountInspector = 0;
//         SetAllMiniGamesActive(false);
//         UIManager.Instance.InGameChoiceButtonActive(false);
//         UIManager.Instance.DebugGamePannelActive(false);

//         EnterStandby();
//     }

//     public void NotifyDebugCleared()
//     {
//         if (!flowRunning || State != FlowState.Debug) return;

//         waitingChoice = true;
//         UIManager.Instance.InGameChoiceButtonActive(true);

//         // 깜빡임 시작 로직 복구
//         if (choiceButtons != null)
//         {
//             foreach (var blinker in choiceButtons)
//             {
//                 if (blinker != null) blinker.StartBlink();
//             }
//         }
//     }

//     public void ResolveDebugChoice(bool playIngame)
//     {
//         if (!flowRunning || State != FlowState.Debug || !waitingChoice) return;

//         waitingChoice = false;

//         // 모든 선택 버튼 깜빡임 중지 로직 복구
//         if (choiceButtons != null)
//         {
//             foreach (var blinker in choiceButtons)
//             {
//                 if (blinker != null) blinker.StopBlink();
//             }
//         }

//         UIManager.Instance.InGameChoiceButtonActive(false);
//         UIManager.Instance.DebugGamePannelActive(false);

//         if (playIngame)
//         {
//             State = FlowState.Ingame;
//             activeIngameMiniGame = FindMiniGameByIssue(InGameMiniGames, currentIssue);

//             if (activeIngameMiniGame == null)
//             {
//                 EndTurn();
//                 return;
//             }

//             activeIngameMiniGame.StartGame();
//         }
//         else
//         {
//             UIManager.Instance.NoiseActive(false);

//             bool returned = poolManager.ReturnIssueWithChance(currentIssue, skipReturnChance);
//             if (!returned) 
//             {
//             TrackSolved(currentIssue);
//             clearedCount++;
//             clearedCountInspector = clearedCount;

//             if (TryClearByGoal()) return;
//             }
            

//             EndTurn();
//         }
//     }

//     public void NotifyIngameCleared()
//     {
//         if (!flowRunning || State != FlowState.Ingame) return;

//         clearedCount++;
//         clearedCountInspector = clearedCount;
//         TrackSolved(currentIssue);
//         if (TryClearByGoal()) return;
//         EndTurn();
//     }

//     public void GameClear(bool showClearPanel = true)
//     {
//         StopFlowInternal();
//         State = FlowState.Clear;
//         UIManager.Instance.NoiseActive(false);

//         waitingChoice = false;
//         currentIssue = null;
//         activeDebugMiniGame = null;
//         activeIngameMiniGame = null;

//         SetAllMiniGamesActive(false);
//         UIManager.Instance.InGameChoiceButtonActive(false);
//         UIManager.Instance.DebugGamePannelActive(false);

//         if (showClearPanel)
//             clearEndingController?.PlayClearSequence(solvedIssues);
//     }

//     private void EnterStandby()
//     {
//         State = FlowState.Standby;
//         waitingChoice = false;
//         SetAllMiniGamesActive(false);

//         UIManager.Instance.InGameChoiceButtonActive(false);
//         UIManager.Instance.DebugGamePannelActive(false);

//         if (standbyRoutine != null) StopCoroutine(standbyRoutine);
//         standbyRoutine = StartCoroutine(StandbyMiniGame());
//     }

//     private IEnumerator StandbyMiniGame()
//     {
//         if (!flowRunning) yield break;

//         yield return new WaitForSeconds(standbySeconds);

//         if (!poolManager.TryDrawRandomIssue(out currentIssue))
//         {
//             GameClear();
//             yield break;
//         }
//         TrackAppeared(currentIssue);

//         State = FlowState.Debug;
//         activeDebugMiniGame = FindMiniGameByIssue(DebugMiniGames, currentIssue);

//         if (activeDebugMiniGame == null)
//         {
//             EndTurn();
//             yield break;
//         }
//         UIManager.Instance.LogMessageActive(true);
//         LogManager.Instance.UpdateIssueLog(currentIssue);

//         UIManager.Instance.SetUnityAlert(true);
//         UIManager.Instance.NoiseActive(true);

//         UIManager.Instance.DebugGamePannelActive(true);
//         activeDebugMiniGame.StartGame();
//     }

//     private void EndTurn()
//     {
//         currentIssue = null;
//         activeDebugMiniGame = null;
//         activeIngameMiniGame = null;
//         EnterStandby();
//     }

//     private MiniGame FindMiniGameByIssue(MiniGame[] list, IssueDefinition issue)
//     {
//         if (list == null || issue == null) return null;

//         for (int i = 0; i < list.Length; i++)
//         {
//             MiniGame mg = list[i];
//             if (mg != null && mg.Issue == issue)
//                 return mg;
//         }

//         return null;
//     }

//     private void SetAllMiniGamesActive(bool active)
//     {
//         SetArrayActive(DebugMiniGames, active);
//         SetArrayActive(InGameMiniGames, active);
//     }

//     private void SetArrayActive(MiniGame[] list, bool active)
//     {
//         if (list == null) return;

//         for (int i = 0; i < list.Length; i++)
//         {
//             if (list[i] != null)
//                 list[i].gameObject.SetActive(active);
//         }
//     }

//     private void StopFlowInternal()
//     {
//         flowRunning = false;

//         // 중단 시 깜빡임 중지 로직 복구
//         if (choiceButtons != null)
//         {
//             foreach (var blinker in choiceButtons)
//             {
//                 if (blinker != null) blinker.StopBlink();
//             }
//         }

//         if (standbyRoutine != null)
//         {
//             StopCoroutine(standbyRoutine);
//             standbyRoutine = null;
//         }
//     }
//     private void TrackAppeared(IssueDefinition issue)
//     {
//         if (issue == null) return;
//         string key = GetIssueKey(issue);
//         if (appearedKeys.Add(key)) appearedIssues.Add(issue);
//     }

//     private void TrackSolved(IssueDefinition issue)
//     {
//         if (issue == null) return;
//         string key = GetIssueKey(issue);
//         if (solvedKeys.Add(key)) solvedIssues.Add(issue);
//     }

//     private string GetIssueKey(IssueDefinition issue)
//     {
//         if (issue == null) return string.Empty;
//         if (!string.IsNullOrWhiteSpace(issue.IssueId)) return issue.IssueId;
//         return issue.GetInstanceID().ToString();
//     }
//     private bool TryClearByGoal()
//     {
//         if (clearedCount >= clearGoalCount)
//         {
//             GameClear();
//             return true;
//         }
//         return false;
//     }


// }