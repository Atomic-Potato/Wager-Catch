using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Animator), typeof(RemoteAnimationEvent))]
public class CustomButton : MonoBehaviour
{
    [SerializeField] AnimationClip _buttonPressClip;
    [SerializeField] bool _isOverideButtonSound;
    [SerializeField] SoundManager.Sound _clickSound = SoundManager.Sound.Click_1;
    protected Animator _animator;
    protected Button _button;
    protected RemoteAnimationEvent _remoteAnimationEvent;

    public void Awake()
    {
        _button = GetComponent<Button>();
        _animator = GetComponent<Animator>();
        _remoteAnimationEvent = GetComponent<RemoteAnimationEvent>();
        AddAnimationToOnClickEvents();
    }

    void AddAnimationToOnClickEvents()
    {
        _button.onClick.AddListener(PlayAnimation);
    }
    public void PlayAnimation()
    {
        _animator.Play(_buttonPressClip.name, -1, 0);

        SoundManager.Sound sound = _isOverideButtonSound ? _clickSound : SoundManager.Instance.GlobalButtonClick;
        SoundManager.Instance.PlaySoundAtPosition(Vector2.zero, sound, true);
    }
}
