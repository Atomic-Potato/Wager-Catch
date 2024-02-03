using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPauseHandler : MonoBehaviour
{
    [SerializeField] AudioSource _source;

    void OnEnable()
    {
        _source.Pause();
    }

    void OnDisable()
    {
        _source.UnPause();
    }
}
