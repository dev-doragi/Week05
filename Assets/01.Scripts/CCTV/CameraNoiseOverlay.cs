using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraNoiseOverlay : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image _targetImage;

    [Header("Ambient Noise")]
    [SerializeField] private Sprite[] _ambientNoiseSprites;
    [SerializeField] private bool _playAmbientRandomly = true;
    [SerializeField] private float _ambientShowDuration = 0.08f;
    [SerializeField] private float _ambientVisibleAlpha = 0.2f;
    [SerializeField] private float _ambientMinRandomDelay = 0.8f;
    [SerializeField] private float _ambientMaxRandomDelay = 2.5f;

    [Header("Switch Noise")]
    [SerializeField] private Sprite[] _switchNoiseSprites;
    [SerializeField] private float _switchShowDuration = 0.08f;
    [SerializeField] private float _switchVisibleAlpha = 0.8f;

    private Coroutine _ambientRoutine;
    private Coroutine _playRoutine;
    private int _lastAmbientSpriteIndex = -1;
    private int _lastSwitchSpriteIndex = -1;
    private bool _isPlayingSwitchNoise;

    private void Awake()
    {
        if (_targetImage == null)
            return;

        Sprite initialSprite = GetInitialSprite();

        if (initialSprite != null)
            _targetImage.sprite = initialSprite;

        SetAlpha(0f);
    }

    private void OnEnable()
    {
        if (_playAmbientRandomly)
            StartAmbientNoise();
    }

    private void OnDisable()
    {
        StopAllNoise();
    }

    public void PlaySwitchNoiseOnce()
    {
        if (_targetImage == null || _switchNoiseSprites == null || _switchNoiseSprites.Length == 0)
            return;

        if (_playRoutine != null)
            StopCoroutine(_playRoutine);

        _playRoutine = StartCoroutine(Co_PlaySwitchNoiseOnce());
    }

    public void StartAmbientNoise()
    {
        if (!_playAmbientRandomly)
            return;

        if (_ambientNoiseSprites == null || _ambientNoiseSprites.Length == 0)
            return;

        if (_ambientRoutine != null)
            StopCoroutine(_ambientRoutine);

        _ambientRoutine = StartCoroutine(Co_AmbientNoiseLoop());
    }

    public void StopAmbientNoise()
    {
        if (_ambientRoutine != null)
        {
            StopCoroutine(_ambientRoutine);
            _ambientRoutine = null;
        }

        if (!_isPlayingSwitchNoise)
            SetAlpha(0f);
    }

    public void StopAllNoise()
    {
        if (_ambientRoutine != null)
        {
            StopCoroutine(_ambientRoutine);
            _ambientRoutine = null;
        }

        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        _isPlayingSwitchNoise = false;
        SetAlpha(0f);
    }

    private IEnumerator Co_AmbientNoiseLoop()
    {
        while (true)
        {
            float delay = Random.Range(_ambientMinRandomDelay, _ambientMaxRandomDelay);
            yield return new WaitForSecondsRealtime(delay);

            if (_isPlayingSwitchNoise)
                continue;

            Sprite ambientSprite = GetRandomAmbientSprite();

            if (ambientSprite == null)
                continue;

            _targetImage.sprite = ambientSprite;
            SetAlpha(_ambientVisibleAlpha);

            yield return new WaitForSecondsRealtime(_ambientShowDuration);

            if (!_isPlayingSwitchNoise)
                SetAlpha(0f);
        }
    }

    private IEnumerator Co_PlaySwitchNoiseOnce()
    {
        _isPlayingSwitchNoise = true;

        Sprite switchSprite = GetRandomSwitchSprite();

        if (switchSprite != null)
            _targetImage.sprite = switchSprite;

        SetAlpha(_switchVisibleAlpha);

        yield return new WaitForSecondsRealtime(_switchShowDuration);

        _isPlayingSwitchNoise = false;
        SetAlpha(0f);
        _playRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (_targetImage == null)
            return;

        Color color = _targetImage.color;
        color.a = alpha;
        _targetImage.color = color;
    }

    private Sprite GetInitialSprite()
    {
        if (_ambientNoiseSprites != null && _ambientNoiseSprites.Length > 0)
            return _ambientNoiseSprites[0];

        if (_switchNoiseSprites != null && _switchNoiseSprites.Length > 0)
            return _switchNoiseSprites[0];

        return null;
    }

    private Sprite GetRandomAmbientSprite()
    {
        if (_ambientNoiseSprites == null || _ambientNoiseSprites.Length == 0)
            return null;

        if (_ambientNoiseSprites.Length == 1)
            return _ambientNoiseSprites[0];

        int index = Random.Range(0, _ambientNoiseSprites.Length);

        if (index == _lastAmbientSpriteIndex)
            index = (index + 1) % _ambientNoiseSprites.Length;

        _lastAmbientSpriteIndex = index;
        return _ambientNoiseSprites[index];
    }

    private Sprite GetRandomSwitchSprite()
    {
        if (_switchNoiseSprites == null || _switchNoiseSprites.Length == 0)
            return null;

        if (_switchNoiseSprites.Length == 1)
            return _switchNoiseSprites[0];

        int index = Random.Range(0, _switchNoiseSprites.Length);

        if (index == _lastSwitchSpriteIndex)
            index = (index + 1) % _switchNoiseSprites.Length;

        _lastSwitchSpriteIndex = index;
        return _switchNoiseSprites[index];
    }
}