using UnityEngine;

public class TriggerGame1 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameFlowManager.Instance.NotifyIngameCleared();
            Debug.Log("인게임클리어");
        }
    }
}