using System.Numerics;

namespace Ability
{
    public class Nuke : AbilityBase
    {
        public override void Spawn()
        {
            base.Spawn();
            GameManager.Instance.ActivateSpecialEnd();
            TeamsManager.Instance.KillAllPlayers();
            SoundManager.Instance.PlaySoundAtPosition(UnityEngine.Vector2.zero, SoundManager.Sound.Nuke);
        }
    }
}
