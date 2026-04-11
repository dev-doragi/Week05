using UnityEngine;
using System.Collections.Generic;

public class ToastManager : Singleton<ToastManager>
{
    [Header("UI Reference")]
    [SerializeField] private UI_ToastSender toastSender;

    [Header("Toast Data Assets")]
    [SerializeField] private SO_ToastData introToast;            // 인트로 토스트
    [SerializeField] private List<SO_ToastData> randomToasts;    // 랜덤 상황용 리스트
    [SerializeField] private SO_ToastData blockedPathToast;      // 길 막힘 상황용 토스트

    private bool isBusy = false; // 현재 알림 출력 여부

    protected override void Init()
    {
        if (toastSender == null)
        {
            Debug.LogError("ToastSender가 할당되지 않았습니다.");
            return;
        }

        // 애니메이션 종료 시 플래그 해제
        toastSender.OnAnimationComplete = () => isBusy = false;
    }

    private void Start()
    {
        // 게임 시작 시 인트로 토스트 실행
        SendIntroToast();
    }

    /// <summary>
    /// 인트로 알림 호출 (중복 방지 적용)
    /// </summary>
    public void SendIntroToast()
    {
        if (isBusy || introToast == null) return;
        ExecuteToast(introToast);
    }

    /// <summary>
    /// 랜덤 알림 호출 (중복 방지 적용)
    /// </summary>
    public void SendRandomToast()
    {
        if (isBusy || randomToasts == null || randomToasts.Count == 0) return;

        int randomIndex = Random.Range(0, randomToasts.Count);
        ExecuteToast(randomToasts[randomIndex]);
    }

    /// <summary>
    /// 길 막힘 알림 호출 (중복 방지 적용)
    /// </summary>
    public void SendBlockedPathToast()
    {
        if (isBusy || blockedPathToast == null) return;

        ExecuteToast(blockedPathToast);
    }

    /// <summary>
    /// 실제 UI 표시 및 플래그 설정
    /// </summary>
    private void ExecuteToast(SO_ToastData data)
    {
        isBusy = true;
        toastSender.Show(data);
    }
}