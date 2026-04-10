using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsoleComponentView : MonoBehaviour
{
    [SerializeField] private Image _consoleLogImage_Fail;
    [SerializeField] private Image _consoleLogImage_Success;
    [SerializeField] private TMP_Text _consoleStageIdText;
    [SerializeField] private TMP_Text _consoleStageTitleText;

    [SerializeField] private Transform _missionParent;
    [SerializeField] private GameObject _missionViewPrefab;
    [SerializeField] private List<UI_ConsoleMissionView> _missionViews = new List<UI_ConsoleMissionView>();

    public void Init(string stageId, string stageTitle, List<(int missionIndex, string missionTitle)> missions, int count)
    {
        _consoleStageIdText.text = stageId;
        _consoleStageTitleText.text = stageTitle;

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
        if (_consoleLogImage_Fail != null)
            _consoleLogImage_Fail.gameObject.SetActive(true);

        if (_consoleLogImage_Success != null)
            _consoleLogImage_Success.gameObject.SetActive(false);
    }

    private void CreateMissionViews(int count)
    {
        if (_missionViews == null)
            _missionViews = new List<UI_ConsoleMissionView>();

        ClearMissionViews();

        if (_missionParent == null)
        {
            Debug.LogError("[UI_ConsoleComponentView] Mission Parent is null.");
            return;
        }

        if (_missionViewPrefab == null)
        {
            Debug.LogError("[UI_ConsoleComponentView] Mission View Prefab is null.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(_missionViewPrefab, _missionParent);
            UI_ConsoleMissionView missionView = obj.GetComponent<UI_ConsoleMissionView>();

            if (missionView == null)
            {
                Debug.LogError("[UI_ConsoleComponentView] UI_ConsoleMissionView component is missing on prefab.");
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
                break;
            }
        }
    }
}
