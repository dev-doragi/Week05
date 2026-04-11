using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildRunStartupController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject startupRoot;
    [SerializeField] private TextMeshProUGUI[] messageSlots;

    [Header("Confirm Blink")]
    [SerializeField] private Image confirmBlinkImage;          
    [SerializeField, Range(0f, 255f)] private float minAlpha = 0f;
    [SerializeField, Range(0f, 255f)] private float maxAlpha = 180f;
    [SerializeField] private float blinkCycleSeconds = 1.2f; 

    [Header("Timing")]
    [SerializeField] private float totalSeconds = 8f;

    private bool _running;
    private Coroutine _co;

    private bool _confirmBlinkLockedOff;

    private void Awake()
    {
        if (stageManager == null)
            stageManager = StageManager.Instance;
    }

    public void OnClickBuildAndRun()
    {
        if (_running) return;
        _confirmBlinkLockedOff = true;
        SetConfirmAlpha01(0f);
        _co = StartCoroutine(CoRun());
    }
    private void Update()
    {
        UpdateConfirmBlink();
    }
    private void UpdateConfirmBlink()
    {
        if (confirmBlinkImage == null) return;
        if (_confirmBlinkLockedOff)
        {
            SetConfirmAlpha01(0f);
            return;
        }

        if (!confirmBlinkImage.gameObject.activeInHierarchy)
        {
            SetConfirmAlpha01(0f);
            return;
        }

        float minA = minAlpha / 255f;
        float maxA = maxAlpha / 255f;
        float half = Mathf.Max(0.01f, blinkCycleSeconds * 0.5f);

        float t = Mathf.PingPong(Time.unscaledTime / half, 1f);
        float a = Mathf.Lerp(minA, maxA, t);

        SetConfirmAlpha01(a);
    }

    private void SetConfirmAlpha01(float a01)
    {
        Color c = confirmBlinkImage.color;
        c.a = Mathf.Clamp01(a01);
        confirmBlinkImage.color = c;
    }
    private IEnumerator CoRun()
    {
        _running = true;

        ResetUI();

        List<StageDefinition> stages = CollectStageMoveDefinitions();
        int showCount = Mathf.Min(stages.Count, messageSlots != null ? messageSlots.Length : 0);

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
                ShowMessage(nextIndex, stages[nextIndex]);
                nextIndex++;
            }

            yield return null;
        }

        if (progressSlider != null)
            progressSlider.value = 1f;

        while (nextIndex < showCount)
        {
            ShowMessage(nextIndex, stages[nextIndex]);
            nextIndex++;
        }

        if (startupRoot != null)
            startupRoot.SetActive(false);
        GameManager.Instance?.StartGame();
        //UIManager.Instance.SetCCTVAlert(true);
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
        if (confirmBlinkImage != null)
        SetConfirmAlpha01(0f);
    }

    private void ShowMessage(int index, StageDefinition stageDef)
    {
        if (messageSlots == null || index < 0 || index >= messageSlots.Length) return;
        if (messageSlots[index] == null || stageDef == null) return;

        Transform notifyRoot = messageSlots[index].transform.parent;
        if (notifyRoot != null && !notifyRoot.gameObject.activeSelf)
            notifyRoot.gameObject.SetActive(true);

        messageSlots[index].text = stageDef.StageTitle;
    }

    private List<StageDefinition> CollectStageMoveDefinitions()
    {
        List<StageDefinition> list = new List<StageDefinition>();
        if (stageManager == null || stageManager.Stages == null) return list;

        Stage[] stages = stageManager.Stages;
        for (int i = 0; i < stages.Length; i++)
        {
            Stage s = stages[i];
            if (s == null) continue;

            // StageMove만 사용
            if (s is not StageMove) continue;

            if (s.StageSO == null) continue;
            list.Add(s.StageSO);
        }

        return list;
    }


}
