using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildRunStartupController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ETypePoolManager poolManager;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject startupRoot;
    [SerializeField] private TextMeshProUGUI[] messageSlots;

    [Header("Timing")]
    [SerializeField] private float totalSeconds = 8f;

    private bool _running;
    private Coroutine _co;

    public void OnClickBuildAndRun()
    {
        if (_running) return;
        _co = StartCoroutine(CoRun());
    }

    private IEnumerator CoRun()
    {
        _running = true;

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
            float t = Mathf.Clamp01(elapsed / totalSeconds);

            if (progressSlider != null)
                progressSlider.value = t;

            while (nextIndex < showCount && elapsed >= perMessage * (nextIndex + 1))
            {
                ShowMessage(nextIndex, issues[nextIndex]);
                nextIndex++;
            }

            yield return null;
        }

        if (progressSlider != null)
            progressSlider.value = 1f;

        while (nextIndex < showCount)
        {
            ShowMessage(nextIndex, issues[nextIndex]);
            nextIndex++;
        }

        if (startupRoot != null)
            startupRoot.SetActive(false);
        GameManager.Instance?.StartGame();
        _running = false;
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

    private void ShowMessage(int index, IssueDefinition issue)
    {
        if (messageSlots == null || index < 0 || index >= messageSlots.Length) return;
        if (messageSlots[index] == null || issue == null) return;

        Transform notifyRoot = messageSlots[index].transform.parent;
        if (notifyRoot != null && !notifyRoot.gameObject.activeSelf)
            notifyRoot.gameObject.SetActive(true);

        messageSlots[index].text = $"[{issue.IssueId}] {issue.IssueTitle}";
    }
}
