using UnityEngine;

public class StageManager : MonoBehaviour
{
    public Stage[] Stages;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetAllStagesActive(false);
        InvokeAllStages(Stages);


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
