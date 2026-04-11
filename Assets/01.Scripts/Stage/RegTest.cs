using UnityEngine;

public class RegTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StageManager.Instance.Register(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
