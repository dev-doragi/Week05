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
    [SerializeField] private List<UI_ConsoleMissionView> _missionViews;


    public void Init(string stageId, string stageTitle, List<(int missionIndex, string missionTitle)> missions)
    {
        _consoleStageIdText.text = stageId;
        _consoleStageTitleText.text = stageTitle;
        for (int i = 0; i < missions.Count && i < _missionViews.Count; i++)
        {
            var mission = missions[i];
            _missionViews[i].Init(mission.missionIndex, mission.missionTitle);
        }
    }

    public void IsSuccess(int missionIndex)
    {
        foreach (var mission in _missionViews)
        {
            if (mission.Index == missionIndex)
            {
                mission.IsSuccess();
            }
        }
    }
}

