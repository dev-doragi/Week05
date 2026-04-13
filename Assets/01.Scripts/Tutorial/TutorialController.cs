using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{

    [Header("StartButton")]
    [SerializeField] private GameObject StartButtonAndTitle;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialogueBG;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float typeCharInterval = 0.03f;
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    [Header("Tutorial Locks")]
    [SerializeField] private MinimapRawImageInteractor3D minimapInteractor;
    [SerializeField] private MonoBehaviour[] extraComponentsToDisable;

    [Header("Step Targets")]
    [SerializeField] private RoomSelectionGroup roomSelectionGroup;
    [SerializeField] private RoomTile tutorialRoomToHighlight;
    [SerializeField] private PassageTile tutorialPassageToHighlight;

    [Header("Path Draw")]
    [SerializeField] private LineRenderer firstPathLine;
    [SerializeField] private Transform[] firstPathPoints;
    [SerializeField] private LineRenderer secondPathLine;
    [SerializeField] private Transform[] secondPathPoints;
    [SerializeField] private float pathDrawDuration = 3f;

    [Header("CCTV Tween (UIManager_New 대체)")]
    [SerializeField] private Canvas cctvCanvas;
    [SerializeField] private RectTransform cctvPanelJaein;
    [SerializeField] private RectTransform editorPanel;
    [SerializeField] private int cctvOpenOrder = 101;
    [SerializeField] private int cctvClosedOrder = 99;
    [SerializeField] private float cctvClosedLeft = -2000f;
    [SerializeField] private float cctvClosedRight = 2000f;
    [SerializeField] private float cctvOpenLeft = 0f;
    [SerializeField] private float cctvOpenRight = 0f;
    [SerializeField] private float editorShownX = 0.6f;
    [SerializeField] private float editorHiddenX = 2000.6f;
    [SerializeField] private float cctvTweenDuration = 0.35f;
    [SerializeField] private Ease cctvEase = Ease.OutCubic;

    [Header("Finish")]
    [SerializeField] private GameObject tutorialWindowToHide;
    [SerializeField] private string nextSceneName = "01.MainScene";
    [SerializeField] private float nextSceneDelay = 0.5f;

    [Header("PlayRectTween")]
    [SerializeField] private RectTransform target;
    [SerializeField] private float loatingTime = 5f;
    [SerializeField] private Ease tweenEase = Ease.InOutSine;
    [SerializeField] private Vector2 targetOffsetMin = new Vector2(-781f, -267f);
    [SerializeField] private Vector2 targetOffsetMax = new Vector2(-781f, -267f);
    [SerializeField] private Vector3 targetScale = new Vector3(2.6402f, 2.6402f, 2.6402f);

    [Header("Tutorial Guide Objects")]
    [SerializeField] private GameObject floorRouteArrowObject;
    [SerializeField] private GameObject coachArrowObject;
    [SerializeField] private RoomID tutorialFocusRoomId = RoomID.B1F_JungleStepLower;
    [SerializeField] private bool useRoomTileClickFlow = true;


    private const int STEP_TAB_REQUIRED = 5;
    private const int STEP_DRAW_LINE_1 = 6;
    private const int STEP_FLOOR_ROUTE_RULE = 7;
    private const int STEP_ROOM_HIGHLIGHT = 8;
    private const int STEP_PASSAGE_HIGHLIGHT_AND_LINE_2 = 9;
    private const int STEP_BIND_HINT = 10; 
    private const int STEP_FINAL = 11;    


    private Sequence _cctvSeq;
    private Sequence _rectSeq;
    private Coroutine _typingRoutine;
    private Coroutine _stepActionRoutine;

    private int _stepIndex = -1;
    private bool _isTyping;
    private bool _waitingClick;
    private bool _waitingTab;
    private bool _isFinishing;
    private bool _cctvOpened;

    private void Awake()
    {
        if (lines == null || lines.Length == 0)
            lines = BuildDefaultLines();

        if (minimapInteractor != null)
            minimapInteractor.enabled = false;

        if (extraComponentsToDisable != null)
        {
            for (int i = 0; i < extraComponentsToDisable.Length; i++)
            {
                if (extraComponentsToDisable[i] != null)
                    extraComponentsToDisable[i].enabled = false;
            }
        }

        if (roomSelectionGroup != null)
            roomSelectionGroup.ClearSelection();

        if (tutorialRoomToHighlight != null)
            tutorialRoomToHighlight.SetSelectedVisual(false);

        if (tutorialPassageToHighlight != null)
            tutorialPassageToHighlight.SetHovered(false);

        if (firstPathLine != null) firstPathLine.gameObject.SetActive(false);
        if (secondPathLine != null) secondPathLine.gameObject.SetActive(false);
        dialogueBG.SetActive(false);
        if (floorRouteArrowObject != null) floorRouteArrowObject.SetActive(false);
        if (coachArrowObject != null) coachArrowObject.SetActive(false);
        dialogueBG.SetActive(false);
        ApplyCctvImmediate(false);

    }

    public void StartButton()
    {
        BeginStep(0);
        StartButtonAndTitle.SetActive(false);
        dialogueBG.SetActive(true);
    }

    private void Update()
    {
        if (_isFinishing) return;

        // 추가: 타이핑 중 클릭하면 즉시 전체 문장 출력
        if (_isTyping && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ForceCompleteCurrentLine();
            return;
        }

        if (_waitingTab)
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                _waitingTab = false;
                PlayCctvOpenTween();
                NextStep();
            }
            return;
        }

        if (_waitingClick && !_isTyping)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_stepIndex == STEP_FINAL)
                    StartCoroutine(FinishRoutine());
                else
                    NextStep();
            }
        }
    }
    private void ForceCompleteCurrentLine()
    {
        if (!_isTyping) return;

        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

        _isTyping = false;

        if (dialogueText != null && _stepIndex >= 0 && _stepIndex < lines.Length)
            dialogueText.text = lines[_stepIndex];

        OnTypeCompleteForCurrentStep();
    }
    private void BeginStep(int index)
    {
        if (dialogueText == null) return;
        if (index < 0 || index >= lines.Length) return;

        _stepIndex = index;
        _isTyping = true;
        _waitingClick = false;
        _waitingTab = false;

        if (_typingRoutine != null) StopCoroutine(_typingRoutine);
        if (_stepActionRoutine != null) StopCoroutine(_stepActionRoutine);
        HandleStepEnter(index);

        _typingRoutine = StartCoroutine(TypeLine(lines[index]));
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = string.Empty;

        for (int i = 0; i < line.Length; i++)
        {
            dialogueText.text += line[i];
            yield return new WaitForSeconds(typeCharInterval);
        }

        _isTyping = false;
        OnTypeCompleteForCurrentStep();
    }

    private void OnTypeCompleteForCurrentStep()
    {
        switch (_stepIndex)
        {
            case STEP_TAB_REQUIRED:
                _waitingTab = true;
                break;

            case STEP_DRAW_LINE_1:
                _stepActionRoutine = StartCoroutine(DrawFirstPathThenWaitClick());
                break;

            case STEP_ROOM_HIGHLIGHT:
                HighlightRoom();
                _waitingClick = true;
                break;

            case STEP_PASSAGE_HIGHLIGHT_AND_LINE_2:
                _stepActionRoutine = StartCoroutine(HighlightPassageAndDrawSecondPathThenWaitClick());
                break;

            default:
                _waitingClick = true;
                break;
        }
    }

    private void NextStep()
    {
        BeginStep(_stepIndex + 1);
    }

    private void HighlightRoom()
    {
        if (tutorialRoomToHighlight == null) return;

        if (useRoomTileClickFlow)
        {
            tutorialRoomToHighlight.OnMinimapClicked();
            return;
        }

        if (CameraManager.Instance != null)
            CameraManager.Instance.SelectCameraByRoomId(tutorialFocusRoomId);

        if (roomSelectionGroup != null)
            roomSelectionGroup.SelectRoom(tutorialRoomToHighlight);
        else
            tutorialRoomToHighlight.SetSelectedVisual(true);
    }

    private void HandleStepEnter(int step)
    {
        if (step == STEP_FLOOR_ROUTE_RULE)
        {
            if (floorRouteArrowObject != null) floorRouteArrowObject.SetActive(true);
        }

        if (step == STEP_ROOM_HIGHLIGHT)
        {
            if (floorRouteArrowObject != null) floorRouteArrowObject.SetActive(false);
            if (coachArrowObject != null) coachArrowObject.SetActive(true);
        }

        if (step >= STEP_PASSAGE_HIGHLIGHT_AND_LINE_2)
        {
            if (coachArrowObject != null) coachArrowObject.SetActive(false);
        }
    }

    private IEnumerator DrawFirstPathThenWaitClick()
    {
        yield return DrawPath(firstPathLine, firstPathPoints, pathDrawDuration);
        _waitingClick = true;
    }

    private IEnumerator HighlightPassageAndDrawSecondPathThenWaitClick()
    {
        if (firstPathLine != null) firstPathLine.gameObject.SetActive(false);

        if (tutorialPassageToHighlight != null)
            tutorialPassageToHighlight.SetTutorialBlockedVisual(true);

        yield return DrawPath(secondPathLine, secondPathPoints, pathDrawDuration);
        _waitingClick = true;
    }

    private IEnumerator DrawPath(LineRenderer line, Transform[] points, float duration)
    {
        if (line == null || points == null) yield break;

        List<Vector3> fullPoints = new List<Vector3>(points.Length);
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;
            Vector3 p = line.useWorldSpace ? points[i].position : line.transform.InverseTransformPoint(points[i].position);
            fullPoints.Add(p);
        }

        if (fullPoints.Count < 2) yield break;

        line.gameObject.SetActive(true);

        int count = fullPoints.Count;
        line.positionCount = count;

        Vector3[] full = fullPoints.ToArray();
        Vector3[] draw = new Vector3[count];

        for (int i = 0; i < count; i++)
            draw[i] = full[0];

        line.SetPositions(draw);

        float d = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        while (elapsed < d)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / d);

            float scaled = t * (count - 1);
            int seg = Mathf.FloorToInt(scaled);
            seg = Mathf.Clamp(seg, 0, count - 2);
            float localT = scaled - seg;

            Vector3 head = Vector3.Lerp(full[seg], full[seg + 1], localT);

            for (int i = 0; i < count; i++)
            {
                if (i <= seg) draw[i] = full[i];
                else if (i == seg + 1) draw[i] = head;
                else draw[i] = head;
            }

            line.SetPositions(draw);
            yield return null;
        }

        line.SetPositions(full);
    }

    private void PlayCctvOpenTween()
    {
        if (_cctvOpened) return;
        _cctvOpened = true;

        if (_cctvSeq != null && _cctvSeq.IsActive())
            _cctvSeq.Kill();

        if (cctvCanvas != null)
            cctvCanvas.sortingOrder = cctvOpenOrder;

        float startLeft = cctvPanelJaein != null ? cctvPanelJaein.offsetMin.x : 0f;
        float startRight = cctvPanelJaein != null ? -cctvPanelJaein.offsetMax.x : 0f;
        float endLeft = cctvOpenLeft;
        float endRight = cctvOpenRight;

        _cctvSeq = DOTween.Sequence();

        float progress = 0f;
        _cctvSeq.Join(
            DOTween.To(() => progress, x =>
            {
                progress = x;
                float left = Mathf.Lerp(startLeft, endLeft, progress);
                float right = Mathf.Lerp(startRight, endRight, progress);
                SetCctvLeftRight(left, right);
            }, 1f, cctvTweenDuration).SetEase(cctvEase)
        );

        if (editorPanel != null)
            _cctvSeq.Join(editorPanel.DOAnchorPosX(editorHiddenX, cctvTweenDuration).SetEase(cctvEase));
    }

    private void ApplyCctvImmediate(bool open)
    {
        if (cctvCanvas != null)
            cctvCanvas.sortingOrder = open ? cctvOpenOrder : cctvClosedOrder;

        SetCctvLeftRight(open ? cctvOpenLeft : cctvClosedLeft, open ? cctvOpenRight : cctvClosedRight);

        if (editorPanel != null)
        {
            Vector2 p = editorPanel.anchoredPosition;
            p.x = open ? editorHiddenX : editorShownX;
            editorPanel.anchoredPosition = p;
        }
    }

    private void SetCctvLeftRight(float left, float right)
    {
        if (cctvPanelJaein == null) return;

        Vector2 min = cctvPanelJaein.offsetMin;
        Vector2 max = cctvPanelJaein.offsetMax;

        min.x = left;
        max.x = -right;

        cctvPanelJaein.offsetMin = min;
        cctvPanelJaein.offsetMax = max;
    }

    private IEnumerator FinishRoutine()
    {
        if (_isFinishing) yield break;
        _isFinishing = true;
        _waitingClick = false;
        _waitingTab = false;

        if (tutorialWindowToHide != null)
            tutorialWindowToHide.SetActive(false);

        float tweenDuration = PlayRectTween();
        yield return new WaitForSeconds(tweenDuration + nextSceneDelay);

        SceneManager.LoadScene(nextSceneName);
    }

    public float PlayRectTween()
    {
        if (target == null) return 0f;

        if (_rectSeq != null && _rectSeq.IsActive())
            _rectSeq.Kill();

        target.offsetMin = Vector2.zero;
        target.offsetMax = Vector2.zero;
        target.localScale = Vector3.one;

        Vector2 fromMin = target.offsetMin;
        Vector2 fromMax = target.offsetMax;
        float duration = Mathf.Max(0.1f, loatingTime);

        _rectSeq = DOTween.Sequence();

        _rectSeq.Join(DOTween.To(
            () => 0f,
            t =>
            {
                target.offsetMin = Vector2.LerpUnclamped(fromMin, targetOffsetMin, t);
                target.offsetMax = Vector2.LerpUnclamped(fromMax, targetOffsetMax, t);
            },
            1f,
            duration
        ).SetEase(tweenEase));

        _rectSeq.Join(target.DOScale(targetScale, duration).SetEase(tweenEase));

        return duration;
    }

    private string[] BuildDefaultLines()
    {
        return new[]
        {
            "이번 프로젝트는, 제가 직접 여러분이 만든 빌드를 평가하러 갑니다.",
            "평가 시간은 10시,\n제가 도착했을때 여러분의 게임이 실행되어야합니다.",
            "약간의 사정으로 조금은 늦을 수도 있습니다.",
            "늦어질 때마다, Jlack 메시지로 공지해드리죠.",
            "'큰일이야.'\n'제 시간에 오시면 빌드가 완성되지 않을 것 같은데'",
            "'일단 TAB키를 눌러 CCTV 사용법부터 익히자.'",
            "'코치님은 B1F Cafeteria 부터 시작해서'\n'3F 에 있는 내 위치까지 올라오신다.'",
            "'B1F 에서는 계단을 통해서만 1F에 올라오고,'\n'1F에선 엘레베이터로만 3F로 올라온다.'",
            "'방을 좌클릭해 코치의 위치를 찾을 수 있고,'\n'찾으면 다음 이동 경로를 확인할 수 있다.'",
            "'통로를 우클릭하면 코치의 이동경로를 막을 수 있다.'\n'길이 막히면 코치는 반대편 경로로 우회한다.'",
            "'특정방에 코치가 있을 땐 발을 묶을 수단이 있을 것 같다.'",
            "'코치님을 최대한 지연시키고, 도착 전에 빌드를 끝내자.'"
        };
    }

    private void OnDisable()
    {
        if (_typingRoutine != null) StopCoroutine(_typingRoutine);
        if (_stepActionRoutine != null) StopCoroutine(_stepActionRoutine);

        if (_cctvSeq != null && _cctvSeq.IsActive()) _cctvSeq.Kill();
        if (_rectSeq != null && _rectSeq.IsActive()) _rectSeq.Kill();
    }
}
