using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Space, Header("Audio Properties")]
    [SerializeField, Min(0f)] Vector2 _randomPitchRange = new Vector2(0.5f, 1f); 

    [Space, Header("Clips")]
    [SerializeField] List<AudioClip> _screamClips = new List<AudioClip>();
    [SerializeField] AudioClip _characterDeathClip;

    public float PlayRandomScreamAtPosition(Vector2 position)
    {
        if (_screamClips.Count == 0)
            return 0f;

        int index = UnityEngine.Random.Range(0, _screamClips.Count);
        AudioSource.PlayClipAtPoint(_screamClips[index], position);
        return _screamClips[index].length;
    }

    public float PlayDeathAtPosition(Vector2 position)
    {
        if (_characterDeathClip == null)
            return 0f;
        
        AudioSource.PlayClipAtPoint(_characterDeathClip, position);
        return _characterDeathClip.length;
    }

    public float PlayDeathWithRandomPitchAtPosition(Vector2 position)
    {
        GameObject audioParent = new GameObject("Death Sound");
        audioParent.transform.position = position;

        AudioSource sauce = audioParent.AddComponent<AudioSource>();
        sauce.clip = _characterDeathClip;
        sauce.pitch = UnityEngine.Random.Range(_randomPitchRange.x, _randomPitchRange.y);
        sauce.Play();

        Destroy(audioParent, _characterDeathClip.length);
        return _characterDeathClip.length;
    }
}
