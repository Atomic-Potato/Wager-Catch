namespace Ability
{
    public class Nuke : AbilityBase
    {
        public override void Spawn()
        {
            base.Spawn();

            TeamsManager.Instance.KillAllPlayers();
        }
    }
}
