using UnityEngine;

public class Blocked_Visual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovingPlatform _platform;
    [SerializeField] private SpriteRenderer _sprite;

    [Header("Color Settings")]
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _blockedColor;

    [Header("Blocked Settings")]
    [SerializeField] private float _pulseSpeed;
    [SerializeField] private float _minAlpha;
    [SerializeField] private float _maxAlpha;

    private void Awake()
    {
        if (_platform == null) _platform = GetComponent<MovingPlatform>();
        if (_sprite == null) _sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_platform == null || _sprite == null) return;

        
        if (_platform.canMove)      // 1. Normal State: Original color
        {
            _sprite.color = _normalColor;
        }
        else                        // 2. Blocked State: Pulse alpha
        {
            float alpha = Mathf.PingPong(Time.time * _pulseSpeed, _maxAlpha - _minAlpha) + _minAlpha;

            Color c = _blockedColor;
            c.a = alpha;
            _sprite.color = c;
        }
    }
}
