using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlyPlayer : MonoBehaviour
{
    private Vector2 _cachedStartPos;

    [Header("Movement")]
    public float jumpForce = 5f;
    public float moveSpeed = 3f;

    private Rigidbody2D _rigid;
    [SerializeField] private bool isActive = false;

    public Action OnHitPillar;
    public Action OnHitClear;

    void Awake() => _rigid = GetComponentInChildren<Rigidbody2D>();


    #region Event
    private void OnEnable()
    {
        FlyingGame.OnFlyingGameStart += HandleStart;

        CollisionReporter.OnEnter2D += HandleCollision;
    }

    private void OnDisable()
    {
        FlyingGame.OnFlyingGameStart -= HandleStart;

        CollisionReporter.OnEnter2D -= HandleCollision;
    }
    #endregion

    private void Update()
    {
        if (!isActive) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) _rigid.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

        _rigid.linearVelocityX = moveSpeed;
    }

    public void Init(Vector2 pos, int count)
    {
        Debug.Log("[Flying Game] Player Init");
        transform.position = pos;
        _rigid.linearVelocity = Vector2.zero;
        Stop();

        Invoke("Play", count);
    }

    public void Play()
    {
        _rigid.simulated = true;
        isActive = true;
    }

    public void Retry()
    {
        Debug.Log("[Flying Game] Player crash Pillar. Retry");

        transform.position = _cachedStartPos;
        _rigid.linearVelocity = Vector2.zero;
    }

    public void Stop()
    {
        _rigid.simulated = false;
        isActive = false;
    }

    private void HandleStart(Vector2 pos, int count)
    {
        _cachedStartPos = pos;
        Init(pos, count);
    }

    private void HandleCollision(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Pillar"))
        {
            Retry();
        }
    }
}
