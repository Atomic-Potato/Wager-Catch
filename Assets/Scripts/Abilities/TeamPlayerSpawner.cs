
namespace Ability
{
    public class TeamPlayerSpawner : AbilityBase
    {
        public override void Spawn()
        {
            base.Spawn();

            switch(GameManager.Instance.PlayerTeam)
            {
                case TagsManager.Tag.Runner:
                    TeamsManager.Instance.AddRunner(transform.position);
                    break;
                case TagsManager.Tag.Catcher:
                    TeamsManager.Instance.AddCatcher(transform.position);
                    break;
            }
        }
    }
}
