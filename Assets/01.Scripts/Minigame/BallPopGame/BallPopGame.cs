using UnityEngine;
using UnityEngine.InputSystem;

public class BallPopGame : MiniGame
{
    public GameObject[] balls;

    int _remaining;

    protected override void OnStart()
    {
        foreach (GameObject ball in balls)
            ball.SetActive(true);

        _remaining = balls.Length;
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null) return;

        for (int i = 0; i < balls.Length; i++)
        {
            if (hit.gameObject != balls[i]) continue;

            balls[i].SetActive(false);
            _remaining--;

            if (_remaining <= 0) Clear();
            return;
        }
    }
}