using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Space, Header("Audio Properties")]
    [SerializeField, Min(0f)] Vector2 _randomPitchRange = new Vector2(0.5f, 1f); 

    [Space, Header("Clips")]
    [SerializeField] SoundClip _gunCock;
    [SerializeField] SoundClip _gunBoom;
    [SerializeField] SoundClip _characterDeathClip;
    [SerializeField] SoundClip _grassTouch;
    [SerializeField] RandomSoundClip _screamClips;

    public enum Sound
    {
        Death,
        GunCock,
        GunBoom,
        Scream,
        Grass,
    }

    public float PlaySoundAtPosition(Vector2 position, Sound sound, bool isRandomPitch = false)
    {
        GameObject audioParent = CreateSoundObject();
        AudioSource sauce = CreateDaSauce();
        sauce.Play();
        Destroy(audioParent, sauce.clip.length);
        return sauce.clip.length;

        GameObject CreateSoundObject()
        {
            GameObject parent = new GameObject("Death Sound");
            parent.transform.position = position;
            return parent;
        }
        AudioSource CreateDaSauce()
        {
            ISoundEffectClip sfxClip = GetSoundEffectClip(sound);
            AudioSource source = audioParent.AddComponent<AudioSource>();
            source.clip = sfxClip.GetAudioClip();
            source.volume = sfxClip.GetVolume();
            if (isRandomPitch)
                source.pitch = UnityEngine.Random.Range(_randomPitchRange.x, _randomPitchRange.y);
            return source;
        }
    }

    ISoundEffectClip GetSoundEffectClip(Sound sound)
    {
        switch (sound)
        {
            case Sound.Death:
                return _characterDeathClip;
            case Sound.GunCock:
                return _gunCock;
            case Sound.GunBoom:
                return _gunBoom;
            case Sound.Scream:
                return _screamClips;
            case Sound.Grass:
                return _grassTouch;
            default:
                return null;
        }
    }
}

public interface ISoundEffectClip
{
    AudioClip GetAudioClip();
    float GetVolume();
}

[Serializable]
public class SoundClip : ISoundEffectClip
{
    [SerializeField] AudioClip _clip;
    [SerializeField, Range(0,1f)] float _volume = 1f;

    public AudioClip GetAudioClip()
    {
        return _clip;
    }

    public float GetVolume()
    {
        return _volume;
    }
}

[Serializable]
public class RandomSoundClip : ISoundEffectClip
{
    [SerializeField] List<AudioClip> _clips;
    [SerializeField, Range(0,1f)] float _volume = 1f;

    public AudioClip GetAudioClip()
    {
        int index = UnityEngine.Random.Range(0, _clips.Count);
        return _clips[index];
    }

    public float GetVolume()
    {
        return _volume;
    }
}