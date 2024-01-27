using UnityEngine;
using Pathfinding;

namespace Ability
{
    public class Bomb : AbilityBase
    {
        [SerializeField, Min(0f)] float _explosionRadius = 1f;
        [SerializeField] LayerMask _explosionLayers;
        [SerializeField] bool _isDrawExplosionRadiusGizmos;

        void OnDrawGizmos()
        {
            if (_isDrawExplosionRadiusGizmos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _explosionRadius);
            }
        }

        public override void Spawn()
        {
            base.Spawn();
            ImpulseObjectsInPorximity();
            SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.Explosion, true);
        }

        void ImpulseObjectsInPorximity()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits =  Physics2D.CircleCastAll(mousePosition, _explosionRadius, Vector2.zero, 50f, _explosionLayers);
            foreach (RaycastHit2D hit in hits)
            {
                TeamPlayer teamPlayer = hit.collider.gameObject.GetComponent<TeamPlayer>();
                if (teamPlayer != null)
                    teamPlayer.ImpulseFromPoint(mousePosition);
                else
                {
                    UnitBase unit = hit.collider.gameObject.GetComponent<UnitBase>();
                    unit.ImpulseFromPoint(mousePosition);
                }
            }
        }
    }
}
