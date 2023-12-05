﻿using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class GuardsManager : MonoBehaviour
{
    [SerializeField, Min(0)] int _guardsToCache;
    [SerializeField] Transform _guardsParent;
    [SerializeField] Guard _guardPrefab;
    [SerializeField] PathRequestManager _pathRequestManager;
    [SerializeField] Transform[] _spawnPoints;

    List<Guard> _guards = new List<Guard>();
    List<Guard> _availableGuards = new List<Guard>();
   public List<IDangerousObject> DangersBeingHandled = new List<IDangerousObject>();
    DangerousObjectsManager _dangersManager;

    void Start()
    {
        _dangersManager = DangerousObjectsManager.Instance;
        LoadGuards();
        
        void LoadGuards()
        {
            for (int i=0; i < _guardsToCache; i++)
            {
                Guard guard = Instantiate(_guardPrefab, _spawnPoints[i%_spawnPoints.Length].position, Quaternion.identity, _guardsParent);
                guard.SetupGuard(_spawnPoints[i%_spawnPoints.Length], this, _pathRequestManager);
                _guards.Add(guard);
                _availableGuards.Add(guard);
            }
        }
    }

    void Update()
    {
        // Assign guards to danger objects 
        foreach (IDangerousObject dObject in _dangersManager.SpawnedObjects)
        {
            if (!DangersBeingHandled.Contains(dObject) && _availableGuards.Count > 0)
            {
                Guard guard = _availableGuards[0];
                _availableGuards.Remove(_availableGuards[0]);
                guard.SetTarget(dObject.GetTransform().position, dObject);
                DangersBeingHandled.Add(dObject);
            }
        }

        // Restore avaliable guards
        foreach (Guard g in _guards)
        {
            if (g.CurrentState == Guard.State.OnStandBy && !_availableGuards.Contains(g))
                _availableGuards.Add(g);
        }
    }
}