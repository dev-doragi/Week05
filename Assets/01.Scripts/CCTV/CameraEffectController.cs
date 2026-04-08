using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraEffectController : MonoBehaviour
{
    private static readonly int CameraAnglePropertyId = Shader.PropertyToID("_camera_angle");

    [SerializeField] private RawImage _targetRawImage;
    [SerializeField] private float _minAngle = -0.1f;
    [SerializeField] private float _maxAngle = 0.1f;
    [SerializeField] private float _holdDuration = 1f;
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private bool _playOnStart = true;

    private Material _runtimeMaterial;
    private Coroutine _angleRoutine;

    private void Awake()
    {
        if (_targetRawImage == null)
            return;
    }

    private void Start()
    {
        if (_targetRawImage != null)
            _runtimeMaterial = _targetRawImage.material;

        if (!_playOnStart)
            return;

        StartAngleMotion();
    }

    private void OnDisable()
    {
        StopAngleMotion();
    }

    public void StartAngleMotion()
    {
        if (_runtimeMaterial == null)
            return;

        StopAngleMotion();
        _angleRoutine = StartCoroutine(AngleMotionLoop());
    }

    public void StopAngleMotion()
    {
        if (_angleRoutine != null)
        {
            StopCoroutine(_angleRoutine);
            _angleRoutine = null;
        }
    }

    private IEnumerator AngleMotionLoop()
    {
        _runtimeMaterial.SetFloat(CameraAnglePropertyId, _minAngle);

        while (true)
        {
            yield return new WaitForSeconds(_holdDuration);
            yield return MoveAngle(_minAngle, _maxAngle);

            yield return new WaitForSeconds(_holdDuration);
            yield return MoveAngle(_maxAngle, _minAngle);
        }
    }

    private IEnumerator MoveAngle(float startAngle, float endAngle)
    {
        if (_moveDuration <= 0f)
        {
            _runtimeMaterial.SetFloat(CameraAnglePropertyId, endAngle);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < _moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _moveDuration);
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            _runtimeMaterial.SetFloat(CameraAnglePropertyId, currentAngle);
            yield return null;
        }

        _runtimeMaterial.SetFloat(CameraAnglePropertyId, endAngle);
    }
}