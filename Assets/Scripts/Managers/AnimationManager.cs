using System.Collections;
using Pathfinding;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    #region Global Variables
    [SerializeField] protected Animator animator;

    [Space]
    [Header("Animation Clips")]
    [Tooltip("If no condition is met, then this clip will be played.")]
    [SerializeField] protected AnimationClip defaultClip;
    [Space, Header("Idle")]
    [SerializeField] protected AnimationClip clipIdle_N;
    [SerializeField] protected AnimationClip clipIdle_NE;
    [SerializeField] protected AnimationClip clipIdle_S;
    [SerializeField] protected AnimationClip clipIdle_SE;
    [SerializeField] protected AnimationClip clipIdle_E;
    [Space, Header("Walk")]
    [SerializeField] protected AnimationClip clipWalk_N;
    [SerializeField] protected AnimationClip clipWalk_NE;
    [SerializeField] protected AnimationClip clipWalk_S;
    [SerializeField] protected AnimationClip clipWalk_SE;
    [SerializeField] protected AnimationClip clipWalk_E;

    [Space, Header("Other")]
    [SerializeField] protected TeamPlayer player;
    [SerializeField] protected Transform spriteParent;

    AnimationClip _clipCurrent;
    public AnimationClip CurrentClip => _clipCurrent;
    Coroutine _waitForAnimationCache;
    #endregion

    protected void Awake()
    {
        SetAnimationState(defaultClip);
    }

    protected void Update()
    {
        #region Animation States Handling
        if (player.IsMoving)
        {
            SetAnimationState(GetWalkingAnimationClip(), true);
        }
        else
            SetAnimationState(GetIdleAnimationClip(), true);
            
        #endregion

        #region Sprite Flipping
        if (player.FacingDirection.x < 0f)
        {
            if (spriteParent.localScale.x > 0f)
                spriteParent.localScale = new Vector3(spriteParent.localScale.x * -1f, spriteParent.localScale.y, spriteParent.localScale.z);
        }
        else
        {
            if (spriteParent.localScale.x < 0f)
                spriteParent.localScale = new Vector3(spriteParent.localScale.x * -1f, spriteParent.localScale.y, spriteParent.localScale.z);
        }
        #endregion
    }

    /// <summary>
    /// Switches the current animation clip.
    /// </summary>
    /// <param name="clip">The clip to be played</param>
    /// <param name="length">
    /// If not ommited, 
    /// the function will not swittch states
    /// until length time passes.
    /// </param>
    /// <param name="endCurrent" >
    /// If current is forced to be played till end
    /// then this will overide it and replace it with a new clip
    /// </param>
    protected void SetAnimationState(AnimationClip clip, bool endCurrent = false, float length = -1f)
    {
        if (clip == _clipCurrent)
        {
            return;
        }

        if (endCurrent)
        {
            if (_waitForAnimationCache != null)
            {
                StopCoroutine(_waitForAnimationCache);
                _waitForAnimationCache = null;
            }
        }
        
        if (_waitForAnimationCache == null)
        {
            if (length > 0f)
            {
                animator.Play(clip.name);
                _clipCurrent = clip;
                _waitForAnimationCache = StartCoroutine(WaitForAnimation());
            }
            else
            {
                animator.Play(clip.name);
                _clipCurrent = clip;
            }
        }

        IEnumerator WaitForAnimation()
        {
            yield return new WaitForSeconds(length);
            _waitForAnimationCache = null;
        }
    }

    #region States Conditions
    protected AnimationClip GetWalkingAnimationClip()
    {
        if (IsFacingNorth)
            return clipWalk_N;
        if (IsFacingNorthEast || IsFacingNorthWest)
            return clipWalk_NE;
        if (IsFacingSouth)
            return clipWalk_S;
        if (IsFacingSouthEast || IsFacingSouthWest)
            return clipWalk_SE;
        if (IsFacingEast || IsFacingWest)
            return clipWalk_E;
        return clipWalk_S;
    }

    protected AnimationClip GetIdleAnimationClip()
    {
        if (IsFacingNorth)
            return clipIdle_N;
        if (IsFacingNorthEast || IsFacingNorthWest)
            return clipIdle_NE;
        if (IsFacingSouth)
            return clipIdle_S;
        if (IsFacingSouthEast || IsFacingSouthWest)
            return clipIdle_SE;
        if (IsFacingEast || IsFacingWest)
            return clipIdle_E;
        return clipIdle_S;
    }

    protected bool IsFacingNorth => player.FacingDirection.y > 0f && Mathf.Approximately(player.FacingDirection.x, 0f);
    protected bool IsFacingNorthEast => player.FacingDirection.y > 0f && player.FacingDirection.x > 0f; 
    protected bool IsFacingNorthWest => player.FacingDirection.y > 0f && player.FacingDirection.x < 0f; 
    protected bool IsFacingSouth => player.FacingDirection.y < 0f && Mathf.Approximately(player.FacingDirection.x, 0f); 
    protected bool IsFacingSouthEast => player.FacingDirection.y < 0f && player.FacingDirection.x > 0f; 
    protected bool IsFacingSouthWest => player.FacingDirection.y < 0f && player.FacingDirection.x < 0f; 
    protected bool IsFacingEast => Mathf.Approximately(player.FacingDirection.y, 0f) && player.FacingDirection.x > 0f; 
    protected bool IsFacingWest => Mathf.Approximately(player.FacingDirection.y, 0f) && player.FacingDirection.x < 0f; 
    #endregion

    #region Events
    public void PlayTouchGrassSound()
    {
        SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.Grass, true);
    }

    #endregion
}
