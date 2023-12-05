using Pathfinding;
using UnityEngine;
public class Guard : TeamPlayer
{
    [Space]
    [Header("Guard Properties")]
    [SerializeField, Min(0f)] float _distanceToCatchTarget = 1f;
    [SerializeField] Transform _caughtObjectHoldingPosition;
    [HideInInspector] public State CurrentState = State.OnStandBy;
    Transform _spawnPoint;
    GuardsManager _manger;
    IDangerousObject _dangerousObject;

    public enum State
    {
        OnStandBy,
        Hunting,
        Catching,
    }

    public void SetupGuard(Transform spawnPoint, GuardsManager manager, PathRequestManager pathRequestManager)
    {
        _spawnPoint = spawnPoint;
        _manger = manager;
        this.PathRequestManager = pathRequestManager;
    }

    new void Update()
    {
        base.Update();
        if (CurrentState == State.Hunting && !_isPathRequestSent)
            SendPathRequest();

        if (CurrentState == State.Hunting)
        {
            if (_dangerousObject.IsDeactivated())
                ReturnToSpawn();
            else
            {
                float distanceToTarget = Vector2.Distance(transform.position, (Vector2)_target);
                if (distanceToTarget <= _distanceToCatchTarget)
                    Catch();
            }
        }

        if (CurrentState == State.Catching && _isReachedDestination)
        {
            _manger.DangersBeingHandled.Remove(_dangerousObject);
            _dangerousObject.DestroySelf();
            ReturnToSpawn();
        }
    }

    public void SetTarget(Vector2 target, IDangerousObject danger)
    {
        CurrentState = State.Hunting;
        _target = target;
        _dangerousObject = danger;
    }

    public void ReturnToSpawn()
    {
        CurrentState = State.OnStandBy;
        _target = _spawnPoint.position;
        SendPathRequest();
    }

    void Catch()
    {
        CurrentState = State.Catching;
        _target = _spawnPoint.position;
        _dangerousObject.Deactivate();
        _dangerousObject.SetParent(transform);
        // _dangerousObject.SetLocalPosition(_caughtObjectHoldingPosition.position);
        _dangerousObject.SetPosition(_caughtObjectHoldingPosition.position);
        SendPathRequest();
    }

    public override void Die()
    {
        _manger.DangersBeingHandled.Remove(_dangerousObject);
        _dangerousObject.DestroySelf();
        base.Die();
    }
}
