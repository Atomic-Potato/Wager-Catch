using System.Collections;
using UnityEngine;

public class GuardPoliceEffect : MonoBehaviour
{
    [SerializeField] Color _colorA = Color.red;
    [SerializeField] Color _colorB = Color.blue;
    [SerializeField, Min(0.1f)] float _colorSwitchDelay = .8f;

    [Space]
    [SerializeField] Guard _guard;
    [SerializeField] SpriteRenderer _characterSpriteRenderer;
    [SerializeField] SpriteRenderer _lightSpriteRenderer;
    [SerializeField] AudioSource _sirenAudio;

    [Space]
    [SerializeField] bool _isColorsOnly;

    void Awake()
    {
        if (!_isColorsOnly)
        {
            _sirenAudio.Play();
            _sirenAudio.Pause();
        }
    }

    void Update()
    {
        if (!_isColorsOnly)
        {
            if (_guard.CurrentState != Guard.State.OnStandBy)
                WeeWooWeeWoo();
            else
                MomSaidNoMoreWeeWoo();
        }
        else
        {
            WeeWooWeeWoo();
        }
    }

    Coroutine _weeWooCoroutine;
    void WeeWooWeeWoo()
    {
        if (_weeWooCoroutine == null)
            _weeWooCoroutine = StartCoroutine(WeeWoo());
        
        IEnumerator WeeWoo()
        {
            if (!_isColorsOnly)
                _sirenAudio.UnPause();
            while (true)
            {
                SetColors(_colorA);
                yield return new WaitForSeconds(_colorSwitchDelay);
                SetColors(_colorB);
                yield return new WaitForSeconds(_colorSwitchDelay);
            }
        }

        void SetColors(Color color)
        {
            _characterSpriteRenderer.color = color;
            _lightSpriteRenderer.color = color;
        }
    }

    void MomSaidNoMoreWeeWoo()
    {
        if (_weeWooCoroutine != null)
        {
            if (!_isColorsOnly)
                _sirenAudio.Pause();
            StopCoroutine(_weeWooCoroutine);
            _weeWooCoroutine = null;
        }
    }
}
