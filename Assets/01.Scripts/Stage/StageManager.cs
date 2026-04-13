using System.Collections.Generic;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    public Stage[] Stages;
    private Stage _currStage;
    private int _currentStageIndex = 0;

    private readonly Dictionary<string, GameObject> _registry = new();
    public void Register(GameObject go) => _registry[go.name] = go;
    public void Unregister(GameObject go) => _registry.Remove(go.name);
    public GameObject Get(string name) => _registry.GetValueOrDefault(name);
    public IReadOnlyDictionary<string, GameObject> All => _registry;


    protected override void Init()
    {
        SetAllStagesActive(false);
        InvokeAllStages(Stages);
        _currStage = Stages[_currentStageIndex];
    }

    private void OnEnable()
    {
        Stage.OnClear += NextStage;
    }

    private void OnDisable()
    {
        Stage.OnClear -= NextStage;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //테스트용으로 바로 스테이지 시작
        //StartStages();
    }

    [ContextMenu("Start Stages")]
    // 스테이지 시작
    public void StartStages()
    {
        _currStage.StartStage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetAllStagesActive(bool active)
    {
        SetArrayActive(Stages, active);
    }

    private void SetArrayActive(Stage[] list, bool active)
    {
        if (list == null) return;

        for (int i = 0; i < list.Length; i++)
        {
            if (list[i] != null)
                list[i].gameObject.SetActive(active);
            Debug.Log($"Set {list[i].name} active: {active} SO Title: {list[i].StageSO.StageId}");
        }
    }

    private void InvokeAllStages(Stage[] list)
    {
        if (list == null) return;

            for (int i = 0; i < list.Length; i++)
            {
            if (list[i] != null)
                list[i].InvokeChange();
        }
    }

    private void NextStage()
    {
        ClearRegistry();

        int nextIndex = _currentStageIndex + 1;
        while (nextIndex < Stages.Length && Stages[nextIndex] == null)
            nextIndex++;

        if (nextIndex >= Stages.Length)
        {
            GameManager.Instance.GameClear();
            return;
        }

        _currentStageIndex = nextIndex;
        _currStage = Stages[_currentStageIndex];
        _currStage.StartStage();
    }



    private void ClearRegistry()
    {
        Debug.Log("Clearing registry...");
        foreach (var go in _registry.Values)
        {
            if (go != null)
                Destroy(go);
        }
        _registry.Clear();
    }
}
