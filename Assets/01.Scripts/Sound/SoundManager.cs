using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private SO_BGM _gameBgm;
    [SerializeField] private bool _playGameBgmOnStart = true;
    [SerializeField, Range(0f, 1f)] private float _bgmVolume = 1f;

    [Header("SFX")]
    [SerializeField] private SFXPlayer _sfxPlayer;

    private SO_BGM _currentBgm;

    public SFXPlayer SfxPlayer => _sfxPlayer;

    protected override void Init()
    {
    
    }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (_bgmSource == null)
            _bgmSource = GetComponent<AudioSource>();

        if (_sfxPlayer == null)
            _sfxPlayer = GetComponentInChildren<SFXPlayer>();

        if (_bgmSource != null)
        {
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.volume = _bgmVolume;
        }
    }

    private void Start()
    {
        if (_playGameBgmOnStart)
            PlayGameBgm();
    }

    public void PlayGameBgm()
    {
        PlayBgm(_gameBgm, true);
    }

    public void PlayBgm(SO_BGM bgm, bool loop = true)
    {
        if (_bgmSource == null || bgm == null || bgm._clip == null)
            return;

        if (_currentBgm == bgm && _bgmSource.isPlaying)
            return;

        _currentBgm = bgm;
        _bgmSource.clip = bgm._clip;
        _bgmSource.loop = loop;
        _bgmSource.volume = _bgmVolume;
        _bgmSource.Play();
    }

    public void StopBgm()
    {
        if (_bgmSource == null)
            return;

        _bgmSource.Stop();
        _bgmSource.clip = null;
        _currentBgm = null;
    }

    public void SetBgmVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);

        if (_bgmSource != null)
            _bgmSource.volume = _bgmVolume;
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (_sfxPlayer == null)
            return;

        _sfxPlayer.Play(clip, volume);
    }

    public void PlaySfx(SO_SFX sfx, float volume = 1f)
    {
        if (_sfxPlayer == null)
            return;

        _sfxPlayer.Play(sfx, volume);
    }

    public AudioSource PlayLoopSfx(SO_SFX sfx, float volume = 1f)
    {
        if (_sfxPlayer == null)
            return null;

        return _sfxPlayer.PlayLoop(sfx, volume);
    }

    public void StopLoopSfx(AudioSource source)
    {
        if (_sfxPlayer == null)
            return;

        _sfxPlayer.Stop(source);
    }
}