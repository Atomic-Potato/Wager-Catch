using System.Collections;
using System.Diagnostics;
using Pathfinding;
using UnityEngine;

namespace Ability
{
    public class Banana : AbilityBase, IDangerousObject
    {
        [SerializeField, Min(0f)] float _playerSleepDuration = .25f;
        [SerializeField] LayerMask _teamPlayerLayerMask;
        [SerializeField] SpriteRenderer _sprite;

        bool _isHasSlipped;
        bool _isDeactivated;

        public void AddToDangerousObjectsManager()
        {
            DangerousObjectsManager.Instance.SpawnedObjects.Add(this);
        }

        public void Deactivate()
        {
            _isDeactivated = true;
        }

        public void DestroySelf()
        {
            DangerousObjectsManager.Instance.SpawnedObjects.Remove(this);
            Destroy(gameObject);
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public bool IsDeactivated()
        {
            return _isDeactivated;
        }

        public void SetLocalPosition(Vector2 position)
        {
            transform.localPosition = position;
        }

        public void SetParent(Transform parent)
        {
            transform.parent = parent;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }

        public override void Spawn()
        {
            base.Spawn();
            AddToDangerousObjectsManager();
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (_isDeactivated)
                return;

            if (_isHasSlipped)
                return;

            if (collider.gameObject.tag == TagsManager.Tag.Guard.ToString())
                return;
                
            if (1 << collider.gameObject.layer == _teamPlayerLayerMask)
            {
                TeamPlayer player = collider.gameObject.GetComponent<TeamPlayer>();
                player.Sleep(_playerSleepDuration);
                _isHasSlipped = true;
                _sprite.enabled = false;
                Destroy(gameObject);
            }
        }
    }
}
