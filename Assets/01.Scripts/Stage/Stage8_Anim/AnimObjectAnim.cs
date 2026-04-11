using DG.Tweening;
using UnityEngine;

public class AnimObjectAnim : MonoBehaviour
{
    public enum AnimType { Spin, Shake }

    [SerializeField] private AnimObject _owner;
    [SerializeField] private AnimType _animType = AnimType.Spin;
    [SerializeField] private float _spinDuration = 1.5f;
    [SerializeField] private float _shakeDuration = 0.35f;
    [SerializeField] private float _shakeStrength = 0.08f;
    [SerializeField] private int _shakeVibrato = 20;
    [SerializeField] private Collider2D _col;

    private Tween _tween;
    private Vector3 _baseLocalPos;
    private Quaternion _baseLocalRot;

    private void Awake()
    {
        _baseLocalPos = transform.localPosition;
        _baseLocalRot = transform.localRotation;
    }

    private void OnEnable()
    {
        if (_owner != null) _owner.Cleared += HandleCleared;
        ApplyState();
    }

    private void OnDisable()
    {
        if (_owner != null) _owner.Cleared -= HandleCleared;
        _tween?.Kill();
    }

    private void ApplyState()
    {
        if (_owner != null && _owner.IsCleared) StopAnim();
        else PlayLoop();
    }

    private void HandleCleared() => StopAnim();

    private void PlayLoop()
    {
        _tween?.Kill();

        _tween = _animType switch
        {
            AnimType.Spin => transform
                .DOLocalRotate(new Vector3(0f, 0f, 360f), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1),

            AnimType.Shake => transform
                .DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, 90f, false, true)
                .SetLoops(-1),

            _ => null
        };
    }

    private void StopAnim()
    {
        _tween?.Kill();
        _col.isTrigger = true;
        transform.localPosition = _baseLocalPos;
        transform.localRotation = _baseLocalRot;
    }
}
