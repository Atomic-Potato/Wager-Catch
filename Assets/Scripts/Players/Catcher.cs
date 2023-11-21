using Pathfinding;
using UnityEngine;

public class Catcher : Player
{
    [HideInInspector] public Transform spawnPoint;
    Runner _targetRunner;
    bool _isCatchingTarget;

    new void Update()
    {
        base.Update();
        if (!_isCatchingTarget)
            FindRunnerTarget();
        else if (_targetRunner != null && _targetRunner.IsInSafeZone)
        {
            if (_targetRunner.Catchers.Contains(this))
            {
                _targetRunner.Catchers.Remove(this);
                FindRunnerTarget();
            }
        }
     
        if (!_isPathRequestSent)
            SendPathRequest();
    }

    void FindRunnerTarget()
    {
        _isCatchingTarget = false;
        
        if (TeamsManager.RunnersNotInSafeArea.Count == 0)
        {
            _targetRunner = null;
            _target = spawnPoint.position;
            return;
        }

        Runner target = null;
        Runner closestRunner = null;
        float distanceToClosestRunner = 0f;

        foreach(Runner runner in TeamsManager.RunnersNotInSafeArea)
        {
            UpdateClosestRunner(runner);

            if (runner.Catchers.Count == 0)
            {
                target = runner;
                break;
            }
        }

        if (target == null)
            target = closestRunner;

        if (target != null)
        {
            target.Catchers.Add(this);
            _isCatchingTarget = true;
            _target = target.transform.position;
        }
        else
        {
            _target = spawnPoint.position;
        }

        _targetRunner = target;

        void UpdateClosestRunner(Runner runner)
        {
            if (closestRunner == null)
            {
                closestRunner = runner;
                distanceToClosestRunner = Vector2.Distance(transform.position, runner.transform.position);
            }
            else
            {
                float distanceToRunner = Vector2.Distance(transform.position, runner.transform.position);
                if (distanceToRunner < distanceToClosestRunner)
                {
                    closestRunner = runner;
                    distanceToClosestRunner = distanceToRunner;
                }
            }
        }
    }
}
