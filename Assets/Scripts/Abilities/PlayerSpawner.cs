using System;
using UnityEngine;

namespace Ability
{
    public class PlayerSpawner : AbilityBase
    {
        [SerializeField] Player _playerPrefab;

        public override void Spawn()
        {
            if (GameManager.Instance.PlayerInstance == null)
            {
                base.Spawn();
                Vector2 spawnPoint = GameManager.Instance.PlayerSpawnPoint.position;
                GameManager.Instance.PlayerInstance = Instantiate(_playerPrefab, spawnPoint, Quaternion.identity);
            }
            else
            {
                Destroy();
            }
        }
    }
}
