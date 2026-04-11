using UnityEngine;
using UnityEngine.InputSystem;

public class BallPopGame : MiniGame
{
    [Header("References")]
    public Collider2D spawnArea;
    public GameObject[] balls;

    [Header("Settings")]
    [Range(1, 15)]
    public int ballCount = 5;

    int _remaining;

    protected override void OnStart()
    {
        foreach (GameObject ball in balls)
            ball.SetActive(false);

        int count = Mathf.Min(ballCount, balls.Length);

        for (int i = 0; i < count; i++)
        {
            balls[i].transform.position = GetRandomPositionInCollider();
            balls[i].SetActive(true);
        }

        _remaining = count;
    }

    Vector2 GetRandomPositionInCollider()
    {
        Bounds bounds = spawnArea.bounds;
        Vector2 point;
        int safety = 0;

        do
        {
            point = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
            safety++;
        }
        while (!spawnArea.OverlapPoint(point) && safety < 100);

        return point;
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector3 screenPos = Mouse.current.position.ReadValue();
        screenPos.z = -Camera.main.transform.position.z;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null) return;

        for (int i = 0; i < balls.Length; i++)
        {
            if (!balls[i].activeSelf) continue;
            if (hit.gameObject != balls[i]) continue;

            balls[i].SetActive(false);
            _remaining--;

            if (_remaining <= 0) Clear();
            return;
        }
    }
}