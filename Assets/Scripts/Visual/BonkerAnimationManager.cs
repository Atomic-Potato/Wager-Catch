using Pathfinding;
using UnityEngine;

public class BonkerAnimationManager : MonoBehaviour
{
    [SerializeField] TeamPlayer _teamPlayer;
    [SerializeField] GameObject _bonkerParent;
    [SerializeField] SpriteRenderer _bonkerSprite;
    [SerializeField] AnimationClip _bonkClip;
    [SerializeField] Animator _animator;

    bool _isBonking;

    void Awake()
    {
        _bonkerSprite.enabled = false;
    }

    void Start()
    {
        _teamPlayer.BonkStartBroadcaster.AddListener(StartBonkingAniamtion);
        _teamPlayer.BonkEndBroadcaster.AddListener(EndBonkingAnimation);
    }

    // NOTE: no need to flip the sprite if the object is a child of the "Sprite Parent" 
    // as that one is already flipping based on direction
    void Update()
    {
        if (_isBonking)
            SetBonkerSpriteLayer();
    }
    
    void SetBonkerSpriteLayer()
    {
        _bonkerSprite.sortingOrder = _teamPlayer.FacingDirection.y > 0 ? 9 : 11;
    }

    void StartBonkingAniamtion()
    {
        _isBonking = true;
        _bonkerSprite.enabled = true;
        _animator.Play(_bonkClip.name);
    }

    void EndBonkingAnimation()
    {
        _isBonking = false;
        _bonkerSprite.enabled = false;
        _animator.Play("None");
    }

    #region Events
    void Bonk()
    {
        _teamPlayer.Bonk();
        SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.Bonk);
    }

    void EndBonk()
    {
        _teamPlayer.EndBonk();
    }

    void PlayStretchSound()
    {
        SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.Stretch, true);
    }
    #endregion
}
