using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UI_ToastSender : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image profileImage;  
    [SerializeField] private Outline toastOutline;  
    [SerializeField] private TextMeshProUGUI workspaceNameText;
    [SerializeField] private TextMeshProUGUI senderNameText; 
    [SerializeField] private TextMeshProUGUI messageContextText; 

    [Header("Animation Settings")]
    [SerializeField] private float hiddenX = 650f;
    [SerializeField] private float visibleX = -30f;
    [SerializeField] private float animDuration = 0.5f;

    public Action OnAnimationComplete;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        ResetPosition();
    }

    private void ResetPosition()
    {
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = new Vector2(hiddenX, rectTransform.anchoredPosition.y);
    }

    public void Show(SO_ToastData data)
    {
        if (data == null) return;

        ResetPosition();

        // 데이터 개별 할당
        profileImage.sprite = data.profileIcon;
        workspaceNameText.text = data.workspaceName;
        senderNameText.text = data.senderName;
        messageContextText.text = data.messageContext;

        if (toastOutline != null)
        {
            toastOutline.effectColor = data.outlineColor;
        }

        PlayAnimation(data.displayDuration);
    }

    private void PlayAnimation(float duration)
    {
        DOTween.Kill(rectTransform);
        DOTween.Kill(canvasGroup);

        Sequence seq = DOTween.Sequence();

        // 등장, 대기, 퇴장 애니메이션 로직
        seq.Append(rectTransform.DOAnchorPosX(visibleX, 0.5f).SetEase(Ease.OutBack));
        seq.Join(canvasGroup.DOFade(1f, 0.4f));
        seq.AppendInterval(duration);
        seq.Append(rectTransform.DOAnchorPosX(hiddenX, 0.5f).SetEase(Ease.InBack));
        seq.Join(canvasGroup.DOFade(0f, 0.4f));

        // 애니메이션 시퀀스가 완전히 끝났을 때 콜백 호출
        seq.OnComplete(() =>
        {
            OnAnimationComplete?.Invoke();
        });
    }
}