// ConsoleManager.cs
using UnityEngine;
using System.Collections.Generic;

public class ConsoleManager : MonoBehaviour
{
    // 씬에 있는 StageController들을 에디터에서 등록하거나 자동수집
    [SerializeField] private List<StageController> stageControllers;

    void Awake()
    {
        // 에디터에서 안 꽂아줬으면 씬에서 자동수집
        if (stageControllers == null || stageControllers.Count == 0)
            stageControllers = new List<StageController>(FindObjectsByType<StageController>(FindObjectsSortMode.None));
    }

    void OnEnable()
    {
        StageController.OnStageChanged += HandleStageChanged;
    }

    void OnDisable()
    {
        StageController.OnStageChanged -= HandleStageChanged;
    }

    private void HandleStageChanged(StageController stage)
    {
        if (stage.IsStageClear)
        {
            // TODO: 스테이지 클리어 UI
        }
        else
        {
            // TODO: 미션 진행 UI (stage.Missions 순회해서 각 미션 상태 반영)
        }
    }
}