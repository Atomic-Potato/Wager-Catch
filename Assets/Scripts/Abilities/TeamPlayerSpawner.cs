using UnityEngine;

namespace Ability
{
    public class TeamPlayerSpawner : AbilityBase
    {
        [SerializeField] TeamsManager.Team team;

        public override void Spawn()
        {
            base.Spawn();

            if (team == TeamsManager.Team.Runner)
                TeamsManager.Instance.AddRunner(transform.position);
            else if (team == TeamsManager.Team.Catcher)
                TeamsManager.Instance.AddCatcher(transform.position);
        }
    }
}
