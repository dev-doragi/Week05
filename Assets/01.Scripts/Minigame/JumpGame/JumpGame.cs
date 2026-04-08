using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class JumpGame : MiniGame
{
    [Header("References")]
    public Rigidbody2D _player;
    public Slider _gaugeSlider;
    public Transform _startPlatform;
    public Transform _targetPlatform;

    [Header("Gauge")]
    public float gaugeSpeed = 1.5f;

    [Header("Jump")]
    public float maxJumpForce = 10f;
    public float targetHalfWidth = 1f;

    float _gaugeDir = 1f;
    bool _isJumping;

    void OnEnable() => CollisionReporter.OnEnter2D += HandleCollision;
    void OnDisable() => CollisionReporter.OnEnter2D -= HandleCollision;

    protected override void OnStart() => ResetGame();

    void ResetGame()
    {
        _player.position = _startPlatform.position + Vector3.up * 0.5f;
        _player.linearVelocity = Vector2.zero;
        _gaugeSlider.value = 0f;
        _gaugeDir = 1f;
        _isJumping = false;
    }

    void Update()
    {
        if (_isJumping) return;

        if (Mouse.current.leftButton.isPressed)
        {
            _gaugeSlider.value += _gaugeDir * gaugeSpeed * Time.deltaTime;

            if (_gaugeSlider.value >= 1f) { _gaugeSlider.value = 1f; _gaugeDir = -1f; }
            else if (_gaugeSlider.value <= 0f) { _gaugeSlider.value = 0f; _gaugeDir = 1f; }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            float force = _gaugeSlider.value * maxJumpForce;
            _player.AddForce(new Vector2(1f, 1f).normalized * force, ForceMode2D.Impulse);
            _gaugeSlider.value = 0f;
            _isJumping = true;
        }
    }

    void HandleCollision(Collider2D other)
    {
        if (!_isJumping) return;
        _isJumping = false;

        bool hit = Mathf.Abs(_player.position.x - _targetPlatform.position.x) <= targetHalfWidth;

        if (hit) Clear();
        else
        {
            Debug.Log("[JumpGame] Miss - Retry");
            ResetGame();
        }
    }
}