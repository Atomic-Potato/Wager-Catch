using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class TrailParticlesManager : MonoBehaviour
{
    [SerializeField] ParticleSystem _trailEffect;
    [SerializeField] TeamPlayer _teamPlayer;
    [SerializeField] UnitBase _player;

    ParticleSystem.MinMaxCurve _originalRateOverTime;

    void Awake()
    {
        _originalRateOverTime = _trailEffect.emission.rateOverTime;
    }

    void Update()
    {
        if (!_teamPlayer && !_player)
            return;
            
        bool isMoving = _teamPlayer ? _teamPlayer.IsMoving : _player.IsMoving;
        if (isMoving)
        {
            var emission = _trailEffect.emission;
            emission.rateOverTime = _originalRateOverTime;
        }
        else
        {
            var emission = _trailEffect.emission;
            emission.rateOverTime = 0f;
        }
    }
}
