using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BuildFailEndingController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject endingRoot;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI[] messageSlots;
    [SerializeField] private Button buildButton;
    [SerializeField] private GameObject gameOverImage;

    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private float gameOverFadeDuration = 0.45f;

    private Tween _gameOverFadeTween;

    private bool _buildLocked;


    [Header("Timing")]
    [SerializeField] private float totalSeconds = 8f;

    [Header("Text")]
    [SerializeField] private string failPrefix = "[Build Error] ";

    private bool _running;
    private Coroutine _co;

   public void OnClickBuildButton()
    {
        if (_buildLocked) return;

        _buildLocked = true;
        if (buildButton != null)
            buildButton.interactable = false;

        PlayBuildFailSequence();
    }


    public void PlayBuildFailSequence()
    {
        if (_running) return;

        if (_co != null)
            StopCoroutine(_co);

        _co = StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {

        if (gameOverImage != null)
        gameOverImage.SetActive(false);
        _running = true;

        if (endingRoot != null)
            endingRoot.SetActive(true);

        ResetUI();

        List<EndingRuntimePayload.FailedStageInfo> failedStages = EndingRuntimePayload.ConsumeFailedStages();
        int showCount = Mathf.Min(failedStages.Count, messageSlots != null ? messageSlots.Length : 0);

        float elapsed = 0f;
        int nextIndex = 0;

        if (showCount > 0)
        {
            ShowFailMessage(0, failedStages[0]);
            nextIndex = 1;
        }

        float perMessage = showCount > 1 ? totalSeconds / (showCount - 1) : totalSeconds;

        while (elapsed < totalSeconds)
        {
            elapsed += Time.deltaTime;

            if (progressSlider != null)
                progressSlider.value = Mathf.Clamp01(elapsed / totalSeconds);

            while (nextIndex < showCount && elapsed >= perMessage * nextIndex)
            {
                ShowFailMessage(nextIndex, failedStages[nextIndex]);
                nextIndex++;
            }

            yield return null;
        }

        if (progressSlider != null)
            progressSlider.value = 1f;
            ShowGameOverWithFade();

        while (nextIndex < showCount)
        {
            ShowFailMessage(nextIndex, failedStages[nextIndex]);
            nextIndex++;
        }

        _running = false;
        _co = null;
    }

    private void ResetUI()
    {
        if (progressSlider != null)
            progressSlider.value = 0f;

        if (messageSlots == null) return;

        for (int i = 0; i < messageSlots.Length; i++)
        {
            if (messageSlots[i] == null) continue;

            messageSlots[i].text = string.Empty;

            Transform notifyRoot = messageSlots[i].transform.parent;
            if (notifyRoot != null)
                notifyRoot.gameObject.SetActive(false);
        }
    }

    private void ShowFailMessage(int index, EndingRuntimePayload.FailedStageInfo info)
    {
        if (messageSlots == null || index < 0 || index >= messageSlots.Length) return;
        if (messageSlots[index] == null) return;

        Transform notifyRoot = messageSlots[index].transform.parent;
        if (notifyRoot != null && !notifyRoot.gameObject.activeSelf)
            notifyRoot.gameObject.SetActive(true);

        string id = string.IsNullOrWhiteSpace(info.StageId) ? "STAGE" : info.StageId;
        string title = string.IsNullOrWhiteSpace(info.StageTitle) ? "Unknown Error" : info.StageTitle;

        messageSlots[index].text = string.IsNullOrEmpty(failPrefix)
            ? $"[{id}] {title}"
            : $"{failPrefix}[{id}] {title}";
    }
    private void ShowGameOverWithFade()
    {
        if (gameOverImage != null && !gameOverImage.activeSelf)
            gameOverImage.SetActive(true);

        if (gameOverCanvasGroup == null) return;

        _gameOverFadeTween?.Kill();
        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.DOFade(1f, gameOverFadeDuration).SetEase(Ease.OutSine);
    }
}
