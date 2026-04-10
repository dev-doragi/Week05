using UnityEngine;

public class MissionClearTest : MonoBehaviour
{

    public StageController stage;
    public bool isMissionClear;
    public int missionIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isMissionClear)
        {
            stage.ClearMission(missionIndex);
        }
    }
}
