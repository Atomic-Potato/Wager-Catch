using Pathfinding;
using UnityEngine;

public class Catcher : Player
{
    [HideInInspector] public Transform spawnPoint;
    Runner _targetRunner;
    bool _isCatchingTarget;


    void Update()
    {
        if (!_isCatchingTarget)
            FindRunnerTarget();
        _target = _targetRunner.transform.position;
        if (!_isPathRequestSent)
            SendPathRequest();
    }

    void FindRunnerTarget()
    {
        if (TeamsManager.RunnersNotInSafeArea.Count > 0)
        {
            foreach(Runner runner in TeamsManager.RunnersNotInSafeArea)
            {
                if (runner.Catchers.Count == 0)
                {
                    _isCatchingTarget = true;
                    runner.Catchers.Add(this);
                    _targetRunner = runner;
                    _target = _targetRunner.transform.position;
                    break;
                }
                else
                {
                    // TODO
                }
            }
        }
    }
}
