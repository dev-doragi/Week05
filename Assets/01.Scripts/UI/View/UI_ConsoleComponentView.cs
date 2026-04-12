using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsoleComponentView : MonoBehaviour
{
    [SerializeField] private Image _consoleLogImage;
    [SerializeField] private Sprite _spriteFail;
    [SerializeField] private Sprite _spriteSuccess;

    [SerializeField] private TMP_Text _consoleStageIdText;
    [SerializeField] private TMP_Text _consoleStageTitleText;

    [SerializeField] private Transform _missionParent;
    [SerializeField] private GameObject _missionViewPrefab;
    [SerializeField] private List<UI_ConsoleMissionView> _missionViews = new();

    private readonly HashSet<int> _completedMissionSlots = new();

    public void Init(string stageId, string stageTitle, List<string> missionTitles)
    {
        _consoleStageIdText.text = stageId;
        _consoleStageTitleText.text = stageTitle;

        _completedMissionSlots.Clear();

        InitializeConsoleState();
        CreateMissionViews(missionTitles.Count);

        for (int i = 0; i < missionTitles.Count && i < _missionViews.Count; i++)
        {
            _missionViews[i].Init(i, missionTitles[i]);
        }
    }

    private void InitializeConsoleState()
    {
        if (_consoleLogImage != null && _spriteFail != null)
            _consoleLogImage.sprite = _spriteFail;
    }

    private void CreateMissionViews(int count)
    {
        if (_missionViews == null)
            _missionViews = new List<UI_ConsoleMissionView>();

        ClearMissionViews();

        if (_missionParent == null)
            return;

        if (_missionViewPrefab == null)
            return;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(_missionViewPrefab, _missionParent);
            UI_ConsoleMissionView missionView = obj.GetComponent<UI_ConsoleMissionView>();

            if (missionView == null)
            {
                Destroy(obj);
                continue;
            }

            _missionViews.Add(missionView);
        }
    }

    private void ClearMissionViews()
    {
        if (_missionViews == null)
            return;

        for (int i = 0; i < _missionViews.Count; i++)
        {
            if (_missionViews[i] != null)
                Destroy(_missionViews[i].gameObject);
        }

        _missionViews.Clear();
    }

    public bool TrySetMissionSuccess(int missionSlot)
    {
        if (missionSlot < 0 || missionSlot >= _missionViews.Count)
            return false;

        if (_completedMissionSlots.Add(missionSlot) == false)
            return false;

        UI_ConsoleMissionView missionView = _missionViews[missionSlot];
        if (missionView == null)
            return false;

        missionView.IsSuccess();
        UpdateCompleteVisual();
        return true;
    }

    private void UpdateCompleteVisual()
    {
        if (_completedMissionSlots.Count < _missionViews.Count)
            return;

        if (_consoleLogImage != null && _spriteSuccess != null)
            _consoleLogImage.sprite = _spriteSuccess;
    }

    public Sequence CreateCompleteSequence()
    {
        UpdateCompleteVisual();

        Sequence sequence = DOTween.Sequence()
            .Pause()
            .SetAutoKill(true);

        BuildCompleteAnimation(sequence);

        if (sequence.Duration(false) <= 0f)
            sequence.AppendInterval(0f);

        return sequence;
    }

    private void BuildCompleteAnimation(Sequence sequence)
    {
        RectTransform rectTransform = transform as RectTransform;
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (rectTransform == null)
            return;

        if (canvasGroup == null)
            return;

        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = startPosition + new Vector2(120f, 0f);

        canvasGroup.alpha = 1f;

        sequence.Append(canvasGroup.DOFade(0.25f, 0.1f));
        sequence.Append(canvasGroup.DOFade(1.2f, 0.1f));
        sequence.AppendInterval(0.1f);
        sequence.Append(rectTransform.DOAnchorPos(endPosition, 0.6f).SetEase(Ease.InCubic));
        sequence.Join(canvasGroup.DOFade(0f, 0.6f));
    }

}
