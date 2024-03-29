﻿using Pathfinding;
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
    Transform _targetTransform;

    public enum State
    {
        OnStandBy,
        Hunting,
        Catching,
    }

    public void SetupGuard(Transform spawnPoint, GuardsManager manager, PathRequestManager pathRequestManager, Pathfinding.Grid grid)
    {
        _spawnPoint = spawnPoint;
        _manger = manager;
        PathRequestManager = pathRequestManager;
        Grid = grid;

        _spawnPoint.position = Grid.GetNodeFromWorldPosition((Vector2)_spawnPoint.position).WorldPosition;
    }

    public void ForceUpdatePath()
    {
        if (_target == null)
            return;
        ForceSendPathRequest();
    }

    new void Update()
    {
        base.Update();

        if (_targetTransform != null)
            _target = _targetTransform.position;

        if (CurrentState == State.Hunting && !_isPathRequestSent)
            SendPathRequest();

        if (CurrentState == State.Hunting)
        {
            // This is a weird bug with unity when destroying an object inheriting from an interface
            // so it doesnt really become null, just "marked" as null ig ??
            if (_dangerousObject.ToString() == "null")
            {
                _targetTransform = null;
                CurrentState = State.OnStandBy;
                ReturnToSpawn();
            }
            else if (GetDistanceToTarget() <= _distanceToCatchTarget)
            {
                Catch();
                ReturnToSpawn();
            }
        }

        if (CurrentState == State.Catching && GetDistanceToTarget() < 0.1f)
        {
            CurrentState = State.OnStandBy;
            DestroyTarget();
        }
    }

    public void SetTarget(Transform target, IDangerousObject danger)
    {
        CurrentState = State.Hunting;
        _targetTransform = target;
        _dangerousObject = danger;
    }

    public void ReturnToSpawn()
    {
        _target = _spawnPoint.position;
        SendPathRequest();
    }

    void Catch()
    {
        CurrentState = State.Catching;
        _targetTransform = null;
        GrabDangerousObject();

        void GrabDangerousObject()
        {
            _dangerousObject.Deactivate();
            _dangerousObject.SetParent(transform);
            _dangerousObject.SetPosition(_caughtObjectHoldingPosition.position);
        }
    }

    float GetDistanceToTarget()
    {
        if (_target == null)
            return 0f;
        return Vector2.Distance(transform.position, (Vector2)_target);
    }

    public void DestroyTarget()
    {
        DangerousObjectsManager.Instance.SpawnedObjects.Remove(_dangerousObject);
        _manger.DangersBeingHandled.Remove(_dangerousObject);
        _dangerousObject.DestroySelf();
    }

    public override void Die()
    {
        _manger.DangersBeingHandled.Remove(_dangerousObject);
        _dangerousObject.DestroySelf();
        base.Die();
    }
}
