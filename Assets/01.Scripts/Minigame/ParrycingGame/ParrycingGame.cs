using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ParrycingGame : MiniGame
{
    [Header("References")]
    public Rigidbody2D player;
    public ParrycingObstacleMover obstacle;
    public Text countText;

    [Header("Settings")]
    public int totalObstacles = 10;
    public float jumpForce = 7f;
    public float spawnInterval = 2f;
    public float respawnDelay = 1f;

    int _dodged;

    bool IsGrounded => Mathf.Abs(player.linearVelocity.y) < 0.05f;

    void OnEnable()
    {
        ParrycingObstacleMover.OnDodged += HandleDodged;
        ParrycingObstacleMover.OnHitPlayer += HandleHitPlayer;
    }

    void OnDisable()
    {
        ParrycingObstacleMover.OnDodged -= HandleDodged;
        ParrycingObstacleMover.OnHitPlayer -= HandleHitPlayer;
        StopAllCoroutines();
        obstacle.gameObject.SetActive(false);
    }

    protected override void OnStart()
    {
        _dodged = 0;
        UpdateUI();
        obstacle.gameObject.SetActive(false);
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        if (!IsGrounded) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        player.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void HandleDodged()
    {
        _dodged++;
        UpdateUI();

        if (_dodged >= totalObstacles)
        {
            StopAllCoroutines();
            Clear();
        }
    }

    void HandleHitPlayer()
    {
        StopAllCoroutines();
        //_dodged = 0;
        UpdateUI();
        StartCoroutine(SpawnRoutine());
    }

    void UpdateUI()
    {
        if (countText != null)
            countText.text = $"{_dodged} / {totalObstacles}";
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnInterval);

        while (true)
        {
            obstacle.ResetPosition();
            obstacle.gameObject.SetActive(true);
            yield return new WaitUntil(() => !obstacle.gameObject.activeSelf);
            yield return new WaitForSeconds(respawnDelay);
        }
    }
}