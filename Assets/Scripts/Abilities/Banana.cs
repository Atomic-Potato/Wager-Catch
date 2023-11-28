using System.Collections;
using Pathfinding;
using UnityEngine;

namespace Abilities
{
    public class Banana : AbilityBase
    {
        [SerializeField, Min(0f)] float _playerSleepDuration = .25f;
        [SerializeField] LayerMask _teamPlayerLayerMask;
        [SerializeField] SpriteRenderer _sprite;

        bool _isHasSlipped;

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (_isHasSlipped)
                return;

            if (1 << collider.gameObject.layer == _teamPlayerLayerMask)
            {
                Player player = collider.gameObject.GetComponent<Player>();
                player.Sleep();
                _isHasSlipped = true;
                _sprite.enabled = false;
                StartCoroutine(WakeUpTeamPlayer(player));
            }
        }

        IEnumerator WakeUpTeamPlayer(Player player)
        {
            yield return new WaitForSeconds(_playerSleepDuration);
            player.Wake();
            Destroy(gameObject);
        }
    }
}
