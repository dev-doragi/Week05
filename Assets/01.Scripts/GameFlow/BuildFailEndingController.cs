using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildFailEndingController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ETypePoolManager poolManager;
    [SerializeField] private GameObject endingRoot;      // EndingPannel
    [SerializeField] private Slider progressSlider;       // Ending Fill
    [SerializeField] private TextMeshProUGUI[] messageSlots;

    [Header("Timing")]
    [SerializeField] private float totalSeconds = 8f;

    private bool _running;

    public void PlayBuildFailSequence()
    {
        if (_running) return;
        StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        _running = true;

        // 게임 진행 정지 + 에디터 패널 강제 앞으로
        GameFlowManager.Instance?.GameClear();
        UIManager.Instance?.EditorPannelPopup();
        UIManager.Instance?.InGameChoiceButtonActive(false);
        UIManager.Instance?.DebugGamePannelActive(false);
        UIManager.Instance?.NoiseActive(false);

        if (endingRoot != null) endingRoot.SetActive(true);

        ResetUI();

        List<IssueDefinition> issues = poolManager != null
            ? poolManager.GetSeedIssuesSnapshot()
            : new List<IssueDefinition>();

        int showCount = Mathf.Min(issues.Count, messageSlots != null ? messageSlots.Length : 0);
        float elapsed = 0f;
        int nextIndex = 0;
        float perMessage = showCount > 0 ? totalSeconds / showCount : totalSeconds;

        while (elapsed < totalSeconds)
        {
            elapsed += Time.deltaTime;
            if (progressSlider != null)
                progressSlider.value = Mathf.Clamp01(elapsed / totalSeconds);

            while (nextIndex < showCount && elapsed >= perMessage * (nextIndex + 1))
            {
                ShowFailMessage(nextIndex, issues[nextIndex]);
                nextIndex++;
            }

            yield return null;
        }

        if (progressSlider != null) progressSlider.value = 1f;

        while (nextIndex < showCount)
        {
            ShowFailMessage(nextIndex, issues[nextIndex]);
            nextIndex++;
        }

        _running = false;
    }

    private void ResetUI()
    {
        if (progressSlider != null) progressSlider.value = 0f;
        if (messageSlots == null) return;

        for (int i = 0; i < messageSlots.Length; i++)
        {
            if (messageSlots[i] == null) continue;
            messageSlots[i].text = string.Empty;

            Transform notifyRoot = messageSlots[i].transform.parent;
            if (notifyRoot != null) notifyRoot.gameObject.SetActive(false);
        }
    }

    private void ShowFailMessage(int index, IssueDefinition issue)
    {
        if (messageSlots == null || index < 0 || index >= messageSlots.Length) return;
        if (messageSlots[index] == null || issue == null) return;

        Transform notifyRoot = messageSlots[index].transform.parent;
        if (notifyRoot != null && !notifyRoot.gameObject.activeSelf)
            notifyRoot.gameObject.SetActive(true);

        // 성공 문구 없이 실패 로그만
        messageSlots[index].text = $"[{issue.IssueId}] {issue.IssueTitle}";
    }
}
