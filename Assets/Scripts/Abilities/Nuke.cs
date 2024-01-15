namespace Ability
{
    public class Nuke : AbilityBase
    {
        public override void Spawn()
        {
            base.Spawn();
            GameManager.Instance.ActivateSpecialEnd();
            TeamsManager.Instance.KillAllPlayers();
        }
    }
}
