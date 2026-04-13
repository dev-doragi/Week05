using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] private int _poolSize = 10;
    [SerializeField] private AudioSource sourcePrefab;

    private readonly List<AudioSource> _soundPool = new();
    private float _currentPitch = 1f;

    private void Awake()
    {
        for (int i = 0; i < _poolSize; i++)
            CreateSource();
    }

    public void Play(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        AudioSource source = GetAvailableSource();
        source.loop = false;
        source.clip = null;
        source.volume = volume;
        source.pitch = _currentPitch;
        source.PlayOneShot(clip);
    }

    public void Play(SO_SFX sfx, float volume)
    {
        AudioClip clip = GetRandomClip(sfx);
        if (clip == null)
            return;

        Play(clip, volume);
    }

    public AudioSource PlayLoop(AudioClip clip, float volume)
    {
        if (clip == null)
            return null;

        AudioSource source = GetAvailableSource();
        source.Stop();
        source.loop = true;
        source.clip = clip;
        source.volume = volume;
        source.pitch = _currentPitch;
        source.Play();
        return source;
    }

    public AudioSource PlayLoop(SO_SFX sfx, float volume)
    {
        AudioClip clip = GetRandomClip(sfx);
        if (clip == null)
            return null;

        return PlayLoop(clip, volume);
    }

    public void Stop(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.loop = false;
        source.clip = null;
    }

    public void StopAll()
    {
        foreach (AudioSource source in _soundPool)
        {
            if (source == null)
                continue;

            source.Stop();
            source.loop = false;
            source.clip = null;
        }
    }

    public void SetPitch(float pitch)
    {
        _currentPitch = pitch;

        foreach (AudioSource source in _soundPool)
        {
            if (source != null)
                source.pitch = pitch;
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in _soundPool)
        {
            if (!source.isPlaying)
                return source;
        }

        return CreateSource();
    }

    private AudioSource CreateSource()
    {
        AudioSource source = Instantiate(sourcePrefab, transform);
        source.playOnAwake = false;
        source.loop = false;
        _soundPool.Add(source);
        return source;
    }

    private AudioClip GetRandomClip(SO_SFX sfx)
    {
        if (sfx == null || sfx._clips == null || sfx._clips.Length == 0)
            return null;

        return sfx._clips[Random.Range(0, sfx._clips.Length)];
    }
}