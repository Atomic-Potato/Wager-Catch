﻿using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TeamsManager : Singleton<TeamsManager>
{
    [SerializeField] Catcher _catcherPrefab;
    [SerializeField] Runner _runnerPrefab;
    [SerializeField] Pathfinding.Grid playersGrid;
    [SerializeField] PathRequestManager pathRequestManager;
    [Space]
    [SerializeField] Transform catchersParent;
    [SerializeField] Transform runnersParent;

    [Space, Header("Players Count")]
    [SerializeField, Min(0)] int _runnersCount = 5;
    [SerializeField, Min(0)] int _catchersCount = 3;
    [SerializeField] List<Transform> _catchersSpawnPoints;
    
    List<Catcher> _catchers = new List<Catcher>();
    public List<Catcher> Catchers => _catchers;
    public int CatchersCount => _catchers.Count;
    float _catchersStrengthScale;
    public float CatchersStrengthScale => _catchersStrengthScale; 
    List<Runner> _runners = new List<Runner>();
    public List<Runner> Runners => _runners;
    public int RunnersCount => _runners.Count;
    float _runnersStrengthScale;
    public float RunnersStrengthScale => _runnersStrengthScale;
    [HideInInspector] public List<Runner> RunnersNotInSafeArea = new List<Runner>();

    [HideInInspector] public CustomUnityEvent TeamsCountBroadcaster;

    int _catcherSpawnPointIndex;

    new void Awake()
    {
        base.Awake();
        RunnersNotInSafeArea.Clear();
        TeamsCountBroadcaster = new CustomUnityEvent();
        LoadNewPlayers();
    }

    public List<RunnerStats> GetRunnersStatsList()
    {
        List<RunnerStats> stats = new List<RunnerStats>();
        foreach (Runner runner in _runners)
            stats.Add(new RunnerStats(runner.SprintDuration, runner.SprintDurationBounds, runner.Speed, runner.SpeedBounds));
        return stats;
    }

    public List<CatcherStats> GetCatchersStatsList()
    {
        List<CatcherStats> stats = new List<CatcherStats>();
        foreach (Catcher catcher in _catchers)
            stats.Add(new CatcherStats(catcher.CatchAreaRadius, catcher.CatchAreaRadiusBounds, catcher.Speed, catcher.SpeedBounds));
        return stats;
    }

    int _runnerCounter = 0;
    public void AddRunner(Vector2 position)
    {
        Runner runner = Instantiate(_runnerPrefab, position, Quaternion.identity, runnersParent);
        runner.gameObject.name = "Runner " + _runnerCounter++;  
        runner.TeamsManager = this;
        runner.Grid = playersGrid;
        runner.PathRequestManager = pathRequestManager;
        _runners.Add(runner);
    }

    void LoadRunners()
    {
        for (int i=0; i < _runnersCount; i++)
            AddRunner(GetRandomSafeNode().WorldPosition);
    }

    int _catcherCounter = 0;
    public void AddCatcher(Vector2 position, Transform spawnPoint = null)
    {
        Catcher catcher = Instantiate(_catcherPrefab, position, Quaternion.identity, catchersParent);
        catcher.gameObject.name = "Catcher " + _catcherCounter++;  
        catcher.TeamsManager = this;
        catcher.Grid = playersGrid;
        catcher.PathRequestManager = pathRequestManager;
        catcher.SpawnPoint = spawnPoint != null ? spawnPoint : _catchersSpawnPoints[_catcherSpawnPointIndex++ % _catchersSpawnPoints.Count];
        _catchers.Add(catcher);
    }

    void LoadCatchers()
    {
        for (int i=0; i < _catchersCount; i++)
        {
            Transform spawnPoint = _catchersSpawnPoints[i%_catchersSpawnPoints.Count];
            AddCatcher(spawnPoint.position, spawnPoint);
        }
    }

    #region Calculating Teams Strength
    float GetRunnersTeamStrengthScale()
    {
        if (RunnersCount == 0)
            return 0;
        
        float totalScore = 0f;
        foreach (Runner runner in _runners)
        {
            float speedScore = (runner.Speed - runner.SpeedBounds.x) / (runner.SpeedBounds.y - runner.SpeedBounds.x) * .5f;
            float sprintScore = (runner.SprintDuration - runner.SprintDurationBounds.x) / (runner.SprintDurationBounds.y - runner.SprintDurationBounds.x) * .5f;
            totalScore += speedScore + sprintScore;
        }

        return totalScore / RunnersCount;
    }

    float GetCatchersTeamStrengthScale()
    {
        if (CatchersCount == 0)
            return 0;
        
        float totalScore = 0f;
        foreach (Catcher catcher in _catchers)
        {
            float speedScore = (catcher.Speed - catcher.SpeedBounds.x) / (catcher.SpeedBounds.y - catcher.SpeedBounds.x) * .5f;
            float catchScore = (catcher.CatchAreaRadius - catcher.CatchAreaRadiusBounds.x) / (catcher.CatchAreaRadiusBounds.y - catcher.CatchAreaRadiusBounds.x) * .5f;
            totalScore += speedScore + catchScore;
        }

        return totalScore / CatchersCount;
    }
    #endregion

    public Node GetRandomSafeNode()
    {
        int n = Random.Range(0, playersGrid.SafeNodes.Count);
        return playersGrid.SafeNodes[n];
    }

    public void RemoveRunner(Runner runner)
    {
        _runners.Remove(runner);
        TeamsCountBroadcaster.Invoke();
    }

    public void RemoveCatcher(Catcher catcher)
    {
        _catchers.Remove(catcher);
        TeamsCountBroadcaster.Invoke();
    }

    public void RemovePlayer(TeamPlayer player)
    {
        if (player.GetType() == typeof(Catcher))
            RemoveCatcher((Catcher)player);
        else if (player.GetType() == typeof(Runner))
            RemoveRunner((Runner)player);
    }

    public void KillAllPlayers()
    {   
        // Note:    Die() removes a player from the list in the teams manager
        //          So we have to create a copy of the players list to iterate over
        //          rather than the main players list

        List<TeamPlayer> players = new List<TeamPlayer>(_catchers);
        players.AddRange(_runners);
        TeamsCountBroadcaster.IsActive = false;

        foreach (TeamPlayer p in players)
            p.Die();
        
        TeamsCountBroadcaster.IsActive = true;
        TeamsCountBroadcaster.Invoke();
    }

    public void LoadNewPlayers()
    {
        RemoveCurrentPlayers();

        LoadRunners();
        LoadCatchers();

        _runnersStrengthScale = GetRunnersTeamStrengthScale();
        _catchersStrengthScale = GetCatchersTeamStrengthScale();

        void RemoveCurrentPlayers()
        {
            List<TeamPlayer> players = new List<TeamPlayer>(_catchers);
            players.AddRange(_runners);
            foreach (TeamPlayer p in players)
                Destroy(p.gameObject);
            _catchers.Clear();
            _runners.Clear();
        }
    }

    public void ForceUpdateAllPlayersPaths()
    {
        foreach (Runner runner in _runners)
        {
            runner.ForceSendPathRequest();
        }

        foreach (Catcher catcher in _catchers)
        {
            catcher.ForceSendPathRequest();
        }
    }

    public void GentlemenNowWePanik()
    {
        foreach (Runner runner in _runners)
            runner.Panik();
        foreach (Catcher catcher in _catchers)
            catcher.Panik();
    }

    public void AllIsFineChaps()
    {
        foreach (Runner runner in _runners)
            runner.Kalm();
        foreach (Catcher catcher in _catchers)
            catcher.Kalm();
    }
}