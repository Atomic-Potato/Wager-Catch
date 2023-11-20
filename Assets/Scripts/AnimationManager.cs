using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    #region Inspector
    [SerializeField] Animator animator;

    [Space]
    [Header("Animation Clips")]
    [Tooltip("If no condition is met, then this clip will be played.")]
    [SerializeField] AnimationClip defaultClip;
    [Space, Header("Idle")]
    [SerializeField] AnimationClip clipIdle_N;
    [SerializeField] AnimationClip clipIdle_NE;
    [SerializeField] AnimationClip clipIdle_S;
    [SerializeField] AnimationClip clipIdle_SE;
    [SerializeField] AnimationClip clipIdle_E;
    [Space, Header("Walk")]
    [SerializeField] AnimationClip clipWalk_N;
    [SerializeField] AnimationClip clipWalk_NE;
    [SerializeField] AnimationClip clipWalk_S;
    [SerializeField] AnimationClip clipWalk_SE;
    [SerializeField] AnimationClip clipWalk_E;

    [Space, Header("Other")]
    [SerializeField] TestUnit testUnit;
    [SerializeField] Transform spriteParent;
    #endregion

    #region Global Variables
    AnimationClip _clipCurrent;
    public AnimationClip CurrentClip => _clipCurrent;
    Coroutine _waitForAnimationCache;
    #endregion

    void Awake()
    {
        SetAnimationState(defaultClip);
    }

    void Update()
    {
        #region Animation States Handling
        if (testUnit.IsMoving)
        {
            SetAnimationState(GetWalkingAnimationClip(), true);
        }
        else
            SetAnimationState(GetIdleAnimationClip(), true);
            
        #endregion

        #region Sprite Flipping
        if (testUnit.FacingDirection.x < 0f)
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
    void SetAnimationState(AnimationClip clip, bool endCurrent = false, float length = -1f)
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
    AnimationClip GetWalkingAnimationClip()
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

    AnimationClip GetIdleAnimationClip()
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

    bool IsFacingNorth => testUnit.FacingDirection.y > 0f && Mathf.Approximately(testUnit.FacingDirection.x, 0f);
    bool IsFacingNorthEast => testUnit.FacingDirection.y > 0f && testUnit.FacingDirection.x > 0f; 
    bool IsFacingNorthWest => testUnit.FacingDirection.y > 0f && testUnit.FacingDirection.x < 0f; 
    bool IsFacingSouth => testUnit.FacingDirection.y < 0f && Mathf.Approximately(testUnit.FacingDirection.x, 0f); 
    bool IsFacingSouthEast => testUnit.FacingDirection.y < 0f && testUnit.FacingDirection.x > 0f; 
    bool IsFacingSouthWest => testUnit.FacingDirection.y < 0f && testUnit.FacingDirection.x < 0f; 
    bool IsFacingEast => Mathf.Approximately(testUnit.FacingDirection.y, 0f) && testUnit.FacingDirection.x > 0f; 
    bool IsFacingWest => Mathf.Approximately(testUnit.FacingDirection.y, 0f) && testUnit.FacingDirection.x < 0f; 
    #endregion

    
}
