using Pathfinding;
using UnityEngine;

public class CatcherAnimationManager : AnimationManager
{
    [Space, Header("Other Clips")]
    [SerializeField] AnimationClip _clip_Bonked;

    void Start()
    {
        ((Catcher)player).BonkedBroadcaster.AddListener(PlayBonkedClip);
    }

    new void  Update()
    {
        if (!player.IsSleeping)
            base.Update();
    }

    void PlayBonkedClip()
    {
        SetAnimationState(_clip_Bonked, true);
    }
}
