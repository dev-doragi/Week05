using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClearEndingController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject endingRoot;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI[] messageSlots;
    [SerializeField] private Button buildButton;
    [SerializeField] private GameObject gameClearImage;

    [Header("Fade")]
    [SerializeField] private CanvasGroup gameClearCanvasGroup;
    [SerializeField] private float gameClearFadeDuration = 0.45f;

    [Header("Timing")]
    [SerializeField] private float totalSeconds = 8f;

    [Header("Text")]
    [SerializeField] private string clearPrefix = "[Build Success] ";

    private bool _running;
    private bool _buildLocked;
    private Coroutine _co;
    private Tween _fadeTween;

    public void OnClickBuildButton()
    {
        if (_buildLocked) return;

        _buildLocked = true;
        if (buildButton != null)
            buildButton.interactable = false;

        PlayClearSequence();
    }

    public void PlayClearSequence()
    {
        if (_running) return;

        if (_co != null)
            StopCoroutine(_co);

        _co = StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        _running = true;

        if (gameClearImage != null)
            gameClearImage.SetActive(false);

        if (endingRoot != null)
            endingRoot.SetActive(true);

        ResetUI();

        List<EndingRuntimePayload.ClearedStageInfo> clearedStages = EndingRuntimePayload.ConsumeClearedStages();
        int showCount = Mathf.Min(clearedStages.Count, messageSlots != null ? messageSlots.Length : 0);

        float elapsed = 0f;
        int nextIndex = 0;

        if (showCount > 0)
        {
            ShowClearMessage(0, clearedStages[0]);
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
                ShowClearMessage(nextIndex, clearedStages[nextIndex]);
                nextIndex++;
            }

            yield return null;
        }

        if (progressSlider != null)
            progressSlider.value = 1f;

        ShowGameClearWithFade();

        while (nextIndex < showCount)
        {
            ShowClearMessage(nextIndex, clearedStages[nextIndex]);
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

    private void ShowClearMessage(int index, EndingRuntimePayload.ClearedStageInfo info)
    {
        if (messageSlots == null || index < 0 || index >= messageSlots.Length) return;
        if (messageSlots[index] == null) return;

        Transform notifyRoot = messageSlots[index].transform.parent;
        if (notifyRoot != null && !notifyRoot.gameObject.activeSelf)
            notifyRoot.gameObject.SetActive(true);

        string id = string.IsNullOrWhiteSpace(info.StageId) ? "STAGE" : info.StageId;
        string title = string.IsNullOrWhiteSpace(info.StageTitle) ? "Clear" : info.StageTitle;

        messageSlots[index].text = string.IsNullOrEmpty(clearPrefix)
            ? $"[{id}] {title}"
            : $"{clearPrefix}[{id}] {title}";
    }

    private void ShowGameClearWithFade()
    {
        if (gameClearImage != null && !gameClearImage.activeSelf)
            gameClearImage.SetActive(true);

        if (gameClearCanvasGroup == null) return;

        _fadeTween?.Kill();
        gameClearCanvasGroup.alpha = 0f;
        _fadeTween = gameClearCanvasGroup.DOFade(1f, gameClearFadeDuration).SetEase(Ease.OutSine);
    }

    private void OnDisable()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;

        if (_fadeTween != null && _fadeTween.IsActive())
            _fadeTween.Kill();
    }
}
