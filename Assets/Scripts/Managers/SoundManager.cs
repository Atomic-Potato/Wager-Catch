using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] List<AudioClip> _screamClips = new List<AudioClip>();


    public float PlayRandomScreamAtPosition(Vector2 position)
    {
        if (_screamClips.Count == 0)
            return 0f;
            
        int index = UnityEngine.Random.Range(0, _screamClips.Count);
        AudioSource.PlayClipAtPoint(_screamClips[index], position);
        return _screamClips[index].length;
    }
}
