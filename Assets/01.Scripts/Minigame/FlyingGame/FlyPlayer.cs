using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Rendering.ShadowCascadeGUI;

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

    public void Init(Vector2 pos)
    {
        transform.position = pos;
        _rigid.linearVelocity = Vector2.zero;
        _rigid.simulated = true;
        isActive = true;
    }

    public void Stop()
    {
        _rigid.simulated = false;
        isActive = false;
    }

    private void HandleStart(Vector2 pos)
    {
        _cachedStartPos = pos;
        Init(pos);
    }

    private void HandleCollision(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Pillar"))
        {
            Init(_cachedStartPos);
        }
    }
}
