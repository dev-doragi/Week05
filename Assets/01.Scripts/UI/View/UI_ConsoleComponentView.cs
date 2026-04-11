using System.Collections.Generic;
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
    [SerializeField] private List<UI_ConsoleMissionView> _missionViews = new List<UI_ConsoleMissionView>();

    private int _successMissionCount = 0;

    public void Init(string stageId, string stageTitle, List<(int missionIndex, string missionTitle)> missions, int count)
    {
        _consoleStageIdText.text = stageId;
        _consoleStageTitleText.text = stageTitle;
        _successMissionCount = 0;

        InitializeConsoleState();
        CreateMissionViews(count);

        for (int i = 0; i < missions.Count && i < _missionViews.Count; i++)
        {
            var mission = missions[i];
            _missionViews[i].Init(mission.missionIndex, mission.missionTitle);
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

        if (_missionParent == null) return;
        if (_missionViewPrefab == null) return;

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

    public void IsSuccess(int missionIndex)
    {
        foreach (var mission in _missionViews)
        {
            if (mission != null && mission.Index == missionIndex)
            {
                mission.IsSuccess();
                _successMissionCount++;
                CheckAllComplete();
                break;
            }
        }
    }

    private void CheckAllComplete()
    {
        if (_successMissionCount >= _missionViews.Count)
        {
            if (_consoleLogImage != null && _spriteSuccess != null)
                _consoleLogImage.sprite = _spriteSuccess;
        }
    }
}