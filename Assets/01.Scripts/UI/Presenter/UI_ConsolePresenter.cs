using System.Collections.Generic;
using UnityEngine;

// 뷰(View)와 모델(Controller)의 연결 다리 역할.
public class UI_ConsolePresenter : MonoBehaviour
{
    // 인스펙터 할당용 변수. 미할당 시 Awake에서 탐색.
    [SerializeField] private UI_ConsoleComponentView _view;
    [SerializeField] private StageController _stageController;

    // 현재 화면에 그릴 스테이지 데이터.
    private StageDefinition _currentStageDefinition;

    private void Awake()
    {
        // 컴포넌트 자동 할당. 방어적 코드.
        _view ??= FindFirstObjectByType<UI_ConsoleComponentView>();
        _stageController ??= FindFirstObjectByType<StageController>();
    }

    private void OnEnable()
    {
        // 이벤트 구독. 스테이지 변경 감지.
        StageController.OnStageChanged += HandleStageChanged;

        // 활성화 시 즉시 화면 갱신.
        RenderCurrentStage();
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제. 메모리 누수 방지.
        StageController.OnStageChanged -= HandleStageChanged;
    }

    // 현재 할당된 컨트롤러 기준으로 화면 그리기.
    public void RenderCurrentStage()
    {
        // 컨트롤러 누락 시 화면 초기화.
        if (_stageController == null)
        {
            Clear();
            return;
        }

        Render(_stageController);
    }

    // 컨트롤러 데이터를 뷰에 전달. 오버로딩.
    public void Render(StageController stageController)
    {
        if (stageController == null)
        {
            Clear();
            return;
        }

        _stageController = stageController;
        _currentStageDefinition = stageController.StageDef;

        // 1. 스테이지 기본 정보 렌더링.
        Render(_currentStageDefinition);
        // 2. 런타임 미션 달성 상태 덮어쓰기.
        ApplyMissionState(stageController.Missions);
    }

    // 스테이지 정의(데이터)를 뷰에 전달. 오버로딩.
    public void Render(StageDefinition stageDefinition)
    {
        _currentStageDefinition = stageDefinition;

        if (_view == null)
            return;

        if (_currentStageDefinition == null)
        {
            Clear();
            return;
        }

        // 뷰에 넘길 미션 데이터 가공.
        List<(int missionIndex, string missionTitle)> missionData = BuildMissionData(_currentStageDefinition);

        // 뷰 초기화 함수 호출.
        _view.Init(
            _currentStageDefinition.StageId,
            _currentStageDefinition.StageTitle,
            missionData,
            missionData.Count);
    }

    // 특정 미션 성공 처리. 뷰 업데이트.
    public void SetMissionSuccess(int missionArrayIndex)
    {
        if (_view == null || _currentStageDefinition == null)
            return;

        StageMission[] missions = _currentStageDefinition.Missions;
        if (missions == null)
            return;

        // 인덱스 범위 초과 예외 처리.
        if (missionArrayIndex < 0 || missionArrayIndex >= missions.Length)
            return;

        // 실제 미션 고유 인덱스 추출 및 뷰 전달.
        int missionIndex = missions[missionArrayIndex].missionIndex;
        _view.IsSuccess(missionIndex);
    }

    // 뷰 데이터 비우기.
    public void Clear()
    {
        if (_view == null)
            return;

        _currentStageDefinition = null;

        // 빈 값 전달하여 화면 리셋.
        _view.Init(string.Empty, string.Empty, new List<(int missionIndex, string missionTitle)>(), 0);
    }

    // 미션 데이터 리스트 생성. 뷰 포맷에 맞게 가공.
    private List<(int missionIndex, string missionTitle)> BuildMissionData(StageDefinition stageDefinition)
    {
        List<(int missionIndex, string missionTitle)> missionData = new List<(int missionIndex, string missionTitle)>();

        if (stageDefinition == null || stageDefinition.Missions == null)
            return missionData;

        // 튜플 형태로 추출 및 저장.
        foreach (StageMission mission in stageDefinition.Missions)
        {
            missionData.Add((mission.missionIndex, mission.missionTitle));
        }

        return missionData;
    }

    // 현재 런타임 미션 진행 상황을 뷰에 동기화.
    private void ApplyMissionState(StageMission[] runtimeMissions)
    {
        if (_view == null || _currentStageDefinition == null)
            return;

        if (_currentStageDefinition.Missions == null || runtimeMissions == null)
            return;

        // 배열 길이 차이로 인한 OutOfRange 방지.
        int count = Mathf.Min(_currentStageDefinition.Missions.Length, runtimeMissions.Length);

        for (int i = 0; i < count; i++)
        {
            // 미달성 미션 무시.
            if (runtimeMissions[i].isMissionSuccess == false)
                continue;

            // 달성한 미션만 뷰에 성공 처리.
            int missionIndex = _currentStageDefinition.Missions[i].missionIndex;
            _view.IsSuccess(missionIndex);
        }
    }

    // 스테이지 변경 이벤트 콜백.
    private void HandleStageChanged(StageController changedStage)
    {
        if (changedStage == null)
            return;

        // 추적 중인 스테이지와 다르면 무시.
        if (_stageController != null && changedStage != _stageController)
            return;

        // 새 스테이지 정보로 화면 갱신.
        Render(changedStage);
    }
}