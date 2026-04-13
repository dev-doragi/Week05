using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastManager : Singleton<ToastManager>
{
    [Header("UI Reference")]
    [SerializeField] private UI_ToastSender _toastSender;

    [Header("Dependencies")]
    [SerializeField] private CoachMovementController _coachMovementController;
    [SerializeField] private GimmickManager _gimmickManager;

    [Header("Common Toast Data Assets")]
    [SerializeField] private SO_ToastData _introToast;
    [SerializeField] private List<SO_ToastData> _randomToasts;
    [SerializeField] private List<SO_ToastData> _blockedPathToasts;

    [Header("Floor Toast Data Assets")]
    [SerializeField] private SO_ToastData _coachB1ToF1Toast;
    [SerializeField] private SO_ToastData _coachF1ToF3Toast;
    [SerializeField] private SO_ToastData _coachGoingDownToast;

    [Header("Gimmick Toast Data Assets")]
    [SerializeField] private SO_ToastData _requestInterviewToast;
    [SerializeField] private SO_ToastData _disableF3ButtonToast;
    [SerializeField] private SO_ToastData _promoteSpringMenuToast;

    [Header("Random Toast")]
    [SerializeField] private bool _useRandomToast = true;
    [SerializeField] private float _randomToastMinInterval = 15f;
    [SerializeField] private float _randomToastMaxInterval = 30f;

    private bool _introShown;
    private Coroutine _randomToastRoutine;

    protected override void Init()
    {
        if (_toastSender == null)
            Debug.LogError("ToastSender가 할당되지 않았습니다.");
    }

    protected override void Awake()
    {
        base.Awake();

        if (_coachMovementController == null)
            _coachMovementController = FindFirstObjectByType<CoachMovementController>();
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += HandleGameStart;

        if (_gimmickManager == null)
            _gimmickManager = GimmickManager.Instance;

        if (_gimmickManager != null)
            _gimmickManager.OnGimmickActivated += HandleGimmickActivated;

        if (_coachMovementController != null)
        {
            _coachMovementController.OnCoachMoved += HandleCoachMoved;
            _coachMovementController.OnCoachPathBlocked += HandleCoachPathBlocked;
        }
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= HandleGameStart;

        if (_gimmickManager != null)
            _gimmickManager.OnGimmickActivated -= HandleGimmickActivated;

        if (_coachMovementController != null)
        {
            _coachMovementController.OnCoachMoved -= HandleCoachMoved;
            _coachMovementController.OnCoachPathBlocked -= HandleCoachPathBlocked;
        }
    }

    private void HandleGameStart()
    {
        if (_introShown == false)
        {
            _introShown = true;
            EnqueueToast(_introToast);
        }

        if (_useRandomToast == false)
            return;

        if (_randomToastRoutine != null)
            StopCoroutine(_randomToastRoutine);

        _randomToastRoutine = StartCoroutine(CoRandomToastLoop());
    }

    private IEnumerator CoRandomToastLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(_randomToastMinInterval, _randomToastMaxInterval);
            yield return new WaitForSeconds(waitTime);

            if (_randomToasts == null || _randomToasts.Count == 0)
                continue;

            int index = Random.Range(0, _randomToasts.Count);
            EnqueueToast(_randomToasts[index]);
        }
    }

    private void HandleGimmickActivated(GimmickType gimmickType)
    {
        switch (gimmickType)
        {
            case GimmickType.RequestInterview:
                EnqueueToast(_requestInterviewToast);
                break;

            case GimmickType.DisableF3Button:
                EnqueueToast(_disableF3ButtonToast);
                break;

            case GimmickType.PromoteSpringMenu:
                EnqueueToast(_promoteSpringMenuToast);
                break;
        }
    }

    private void HandleCoachPathBlocked(RoomID from, RoomID to)
    {
        int index = Random.Range(0, _blockedPathToasts.Count);
        EnqueueToast(_blockedPathToasts[index]);
    }

    private void HandleCoachMoved(RoomID previousRoom, RoomID currentRoom)
    {
        if (previousRoom == RoomID.None || currentRoom == RoomID.None)
            return;

        if (HasFloorChanged(previousRoom, currentRoom) == false)
            return;

        string previousFloor = GetFloorKey(previousRoom);
        string currentFloor = GetFloorKey(currentRoom);

        if (previousFloor == "B1F" && currentFloor == "F1")
        {
            EnqueueToast(_coachB1ToF1Toast);
            return;
        }

        if (previousFloor == "F1" && currentFloor == "F3")
        {
            EnqueueToast(_coachF1ToF3Toast);
            return;
        }

        if ((previousFloor == "F3" && currentFloor == "F1") ||
            (previousFloor == "F1" && currentFloor == "B1F"))
        {
            EnqueueToast(_coachGoingDownToast);
        }
    }

    public void SendIntroToast()
    {
        EnqueueToast(_introToast);
    }

    public void SendRandomToast()
    {
        if (_randomToasts == null || _randomToasts.Count == 0)
            return;

        int randomIndex = Random.Range(0, _randomToasts.Count);
        EnqueueToast(_randomToasts[randomIndex]);
    }

    public void SendBlockedPathToast()
    {
        int index = Random.Range(0, _blockedPathToasts.Count);
        EnqueueToast(_blockedPathToasts[index]);
    }

    public void EnqueueToast(SO_ToastData data)
    {
        if (data == null || _toastSender == null)
            return;

        _toastSender.Show(data);
    }

    private bool HasFloorChanged(RoomID from, RoomID to)
    {
        string fromFloor = GetFloorKey(from);
        string toFloor = GetFloorKey(to);

        if (string.IsNullOrEmpty(fromFloor) || string.IsNullOrEmpty(toFloor))
            return false;

        return fromFloor != toFloor;
    }

    private string GetFloorKey(RoomID roomId)
    {
        string roomName = roomId.ToString();
        int underscoreIndex = roomName.IndexOf('_');

        if (underscoreIndex <= 0)
            return string.Empty;

        return roomName.Substring(0, underscoreIndex);
    }
}