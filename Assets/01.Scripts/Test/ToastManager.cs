using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastManager : Singleton<ToastManager>
{
    [Header("UI Reference")]
    [SerializeField] private UI_ToastSender toastSender;

    [Header("Dependencies")]
    [SerializeField] private CoachMovementController coachMovementController;
    [SerializeField] private GimmickManager gimmickManager;

    [Header("Toast Data Assets")]
    [SerializeField] private SO_ToastData introToast;
    [SerializeField] private List<SO_ToastData> randomToasts;
    [SerializeField] private SO_ToastData blockedPathToast;

    [Header("Random Toast")]
    [SerializeField] private bool useRandomToast = true;
    [SerializeField] private float randomToastMinInterval = 15f;
    [SerializeField] private float randomToastMaxInterval = 30f;

    private readonly Queue<SO_ToastData> _toastQueue = new Queue<SO_ToastData>();

    private bool _isShowing;
    private bool _introShown;
    private Coroutine _randomToastRoutine;
    private MapGraph _mapGraph;

    protected override void Init()
    {
        if (toastSender == null)
        {
            Debug.LogError("ToastSender가 할당되지 않았습니다.");
            return;
        }

        toastSender.OnAnimationComplete = HandleToastComplete;
    }

    private void Awake()
    {
        if (coachMovementController == null)
            coachMovementController = FindFirstObjectByType<CoachMovementController>();

        if (gimmickManager == null)
            gimmickManager = GimmickManager.Instance;

        _mapGraph = new MapGraph();
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += HandleGameStart;

        if (gimmickManager != null)
            gimmickManager.OnPathBlocked += HandlePathBlocked;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= HandleGameStart;

        if (gimmickManager != null)
            gimmickManager.OnPathBlocked -= HandlePathBlocked;
    }

    private void HandleGameStart()
    {
        if (_introShown == false)
        {
            _introShown = true;
            EnqueueToast(introToast);
        }

        if (useRandomToast)
        {
            if (_randomToastRoutine != null)
                StopCoroutine(_randomToastRoutine);

            _randomToastRoutine = StartCoroutine(Co_RandomToastLoop());
        }
    }

    private IEnumerator Co_RandomToastLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(randomToastMinInterval, randomToastMaxInterval);
            yield return new WaitForSeconds(waitTime);

            if (randomToasts == null || randomToasts.Count == 0)
                continue;

            int index = Random.Range(0, randomToasts.Count);
            EnqueueToast(randomToasts[index]);
        }
    }

    private void HandlePathBlocked(RoomID from, RoomID to)
    {
        if (blockedPathToast == null)
            return;

        if (coachMovementController == null)
            return;

        RoomID coachRoom = coachMovementController.CurrentRoomId;
        if (coachRoom == RoomID.None)
            return;

        if (IsNearBlockedPath(coachRoom, from, to) == false)
            return;

        EnqueueToast(blockedPathToast);
    }

    private bool IsNearBlockedPath(RoomID coachRoom, RoomID from, RoomID to)
    {
        if (coachRoom == from || coachRoom == to)
            return true;

        if (_mapGraph == null)
            return false;

        IReadOnlyList<RoomID> neighbors = _mapGraph.GetNeighbors(coachRoom);
        for (int i = 0; i < neighbors.Count; i++)
        {
            if (neighbors[i] == from || neighbors[i] == to)
                return true;
        }

        return false;
    }

    public void SendIntroToast()
    {
        EnqueueToast(introToast);
    }

    public void SendRandomToast()
    {
        if (randomToasts == null || randomToasts.Count == 0)
            return;

        int randomIndex = Random.Range(0, randomToasts.Count);
        EnqueueToast(randomToasts[randomIndex]);
    }

    public void SendBlockedPathToast()
    {
        EnqueueToast(blockedPathToast);
    }

    public void EnqueueToast(SO_ToastData data)
    {
        if (data == null || toastSender == null)
            return;

        _toastQueue.Enqueue(data);
        TryShowNextToast();
    }

    private void TryShowNextToast()
    {
        if (_isShowing)
            return;

        if (_toastQueue.Count == 0)
            return;

        SO_ToastData data = _toastQueue.Dequeue();
        _isShowing = true;
        toastSender.Show(data);
    }

    private void HandleToastComplete()
    {
        _isShowing = false;
        TryShowNextToast();
    }
}