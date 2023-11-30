using Pathfinding;
using UnityEngine;

namespace Abilities
{
    public class TeamPlayerDespawner : AbilityBase
    {
        [SerializeField] TagsManager.Tag _teamPlayerTags;

        public override void Spawn()
        {
            base.Spawn();

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePosition, 0.1f);
            Collider2D collider = GetColliderWithPlayerTag();

            if (collider != null)
            {
                Player player = collider.gameObject.GetComponent<Player>();
                TeamsManager.Instance.RemovePlayer(player);
                player.Die();
            }

            Collider2D GetColliderWithPlayerTag()
            {
                foreach (Collider2D col in colliders)
                {
                    TagsManager.Tag colliderTag = TagsManager.GetTagFromString(col.gameObject.tag);
                    if (TagsManager.IsTagOneOfMultipleTags(colliderTag, _teamPlayerTags))
                        return col;
                }
                return null;
            }
        }
    }
}
