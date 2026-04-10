using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private AudioSource sourcePrefab;

    private List<AudioSource> _soundPool = new List<AudioSource>();
    private float _currentPitch = 1f;

    private void Awake()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            AudioSource source = Instantiate(sourcePrefab, transform);
            _soundPool.Add(source);
        }
    }

    public void Play(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        AudioSource source = GetAvailableSource();
        source.volume = volume;
        source.pitch = _currentPitch;
        source.PlayOneShot(clip);
    }

    public void Play(SO_SFX sfx, float volume)
    {
        if (sfx == null || sfx._clips == null || sfx._clips.Length == 0)
            return;

        AudioClip clip = sfx._clips[Random.Range(0, sfx._clips.Length)];
        Play(clip, volume);
    }

    private AudioSource GetAvailableSource()
    {
        foreach (var source in _soundPool)
        {
            if (!source.isPlaying)
                return source;
        }

        return _soundPool[0];
    }

    public void SetPitch(float pitch)
    {
        _currentPitch = pitch;

        foreach (var source in _soundPool)
        {
            if (source != null)
                source.pitch = pitch;
        }
    }
}