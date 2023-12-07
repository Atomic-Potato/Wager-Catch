using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Ability
{
    public class TeamPlayerDespawner : AbilityBase
    {
        public override void Spawn()
        {
            base.Spawn();

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePosition, 0.1f);
            Collider2D collider = GetColliderWithTeamPlayerTag();

            if (collider != null)
            {
                TeamPlayer player = collider.gameObject.GetComponent<TeamPlayer>();
                TeamsManager.Instance.RemovePlayer(player);
                player.Die();
            }
            else
            {
                Item.Restore();
            }

            Collider2D GetColliderWithTeamPlayerTag()
            {
                foreach (Collider2D col in colliders)
                {
                    string colliderTag = col.gameObject.tag;
                    TagsManager.Tag oppositeTeamTag = GameManager.Instance.OppositeTeam;
                    if (oppositeTeamTag == TagsManager.Tag.NuteralPlayer || colliderTag == oppositeTeamTag.ToString())
                        return col;
                }
                return null;
            }
        }
    }
}
