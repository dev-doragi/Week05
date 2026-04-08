using UnityEngine;

public class Test_DebugGame1 : MonoBehaviour
{
    public void PressButtonGame()
    {
        GameFlowManager.Instance.NotifyDebugCleared();
    }
}
