using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.AI;

public class SoundManager : Singleton<SoundManager>
{
    [Space, Header("Global Sounds")]
    [SerializeField] Sound _globalButtonClick = Sound.Click_1;
    public Sound GlobalButtonClick => _globalButtonClick;

    [Space, Header("Audio Properties")]
    [SerializeField, Min(0f)] Vector2 _randomPitchRange = new Vector2(0.5f, 1f); 

    [Space, Header("Clips")]
    [SerializeField] SoundClip _gunCock;
    [SerializeField] SoundClip _gunBoom;
    [SerializeField] SoundClip _characterDeathClip;
    [SerializeField] SoundClip _grassTouch;
    [SerializeField] SoundClip _explosion;
    [SerializeField] SoundClip _nuke;
    [SerializeField] SoundClip _bonk;
    [SerializeField] SoundClip _stretch;
    [SerializeField] RandomSoundClip _screamClips;

    [Space, Header("UI")]
    [SerializeField] SoundClip _click1;
    [SerializeField] SoundClip _click2;

    public enum Sound
    {
        Death,
        GunCock,
        GunBoom,
        Scream,
        Grass,
        Explosion,
        Nuke,
        Bonk,
        Stretch,
        Click_1,
        Click_2,
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
            case Sound.Explosion:
                return _explosion;
            case Sound.Nuke:
                return _nuke;
            case Sound.Bonk:
                return _bonk;
            case Sound.Stretch:
                return _stretch;
            case Sound.Click_1:
                return _click1;
            case Sound.Click_2:
                return _click2;
            default:
                return null;
        }
    }

    public float PlaySoundAtPosition(Vector2 position, Sound sound, bool isRandomPitch = false, bool isAffectedByTimeScale = true)
    {
        GameObject audioParent = CreateSoundObject();
        AudioSource sauce = CreateDaSauce();
        if (sound == Sound.Click_1)
            Debug.Log(sauce.pitch);
        sauce.Play();
        DestroyManager.Instance.Destroy(audioParent, sauce.clip.length, true);
        return sauce.clip.length;

        GameObject CreateSoundObject()
        {
            GameObject parent = new GameObject("Sound: " + sound.ToString());
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
            if (isAffectedByTimeScale)
                source.pitch *= Time.timeScale;
            return source;
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