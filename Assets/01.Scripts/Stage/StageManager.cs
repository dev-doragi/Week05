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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
}
