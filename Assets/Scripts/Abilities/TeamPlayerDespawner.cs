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
            Collider2D collider = Physics2D.OverlapCircle(mousePosition, 0.1f);
            TagsManager.Tag colliderTag = TagsManager.GetTagFromString(collider.gameObject.tag);
            Debug.Log("Used Despawner");
            if (TagsManager.IsTagOneOfMultipleTags(colliderTag, _teamPlayerTags))
            {
                Debug.Log("Has found player");
                Player player = collider.gameObject.GetComponent<Player>();
                TeamsManager.Instance.RemovePlayer(player);
                player.Die();
            }
        }
    }
}
