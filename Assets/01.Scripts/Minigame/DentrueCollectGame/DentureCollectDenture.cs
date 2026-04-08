using UnityEngine;

public class DentureCollectDenture : MonoBehaviour
{
    // Rigidbody2D가 붙어있으면 중력으로 알아서 낙하
    // 뭐에 닿든 무조건 소멸
    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }
}