using UnityEngine;

namespace Abilities
{
    public class PlayerSpawner : AbilityBase
    {
        [SerializeField] Player _playerPrefab;

        public override void Spawn()
        {
            base.Spawn();

            Vector2 _spawnPoint = GameManager.Instance.PlayerSpawnPoint.position;
            Instantiate(_playerPrefab, _spawnPoint, Quaternion.identity);
        }
    }
}
