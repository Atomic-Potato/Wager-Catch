using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class TeamsManager : Singleton<TeamsManager>
{
    [SerializeField] GameObject _catcherPrefab;
    [SerializeField] GameObject _runnerPrefab;
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
    public int CatchersCount => _catchers.Count;
    float _catchersStrengthScale;
    public float CatchersStrengthScale => _catchersStrengthScale; 
    List<Runner> _runners = new List<Runner>();
    public int RunnersCount => _runners.Count;
    float _runnersStrengthScale;
    public float RunnersStrengthScale => _runnersStrengthScale;
    [HideInInspector] public List<Runner> RunnersNotInSafeArea = new List<Runner>();

    [HideInInspector] public CustomUnityEvent TeamsCountBroadcaster;

    int _catcherSpawnPointIndex;

    new void Awake()
    {
        base.Awake();
        LoadRunners();
        LoadCatchers();
        TeamsCountBroadcaster = new CustomUnityEvent();
        _runnersStrengthScale = GetRunnersTeamStrengthScale();
        Debug.Log("Runners team strength: " + _runnersStrengthScale);
    }

    public void AddRunner(Vector2 position)
    {
        GameObject spawnedRunnerObject = Instantiate(_runnerPrefab, position, Quaternion.identity, runnersParent);
        Runner runner = spawnedRunnerObject.GetComponent<Runner>();
        runner.TeamsManager = this;
        runner.grid = playersGrid;
        runner.PathRequestManager = pathRequestManager;
        _runners.Add(runner);
        Debug.Log("Runer stats: \n SPEED: " + runner.Speed + " | SPRINT :" + runner.SprintDuration);
    }

    void LoadRunners()
    {
        for (int i=0; i < _runnersCount; i++)
            AddRunner(GetRandomSafeNode().WorldPosition);
    }

    public void AddCatcher(Vector2 position, Transform spawnPoint = null)
    {
        GameObject spawnedCactherObject = Instantiate(_catcherPrefab, position, Quaternion.identity, catchersParent);
        Catcher catcher = spawnedCactherObject.GetComponent<Catcher>();
        catcher.TeamsManager = this;
        catcher.grid = playersGrid;
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

    float GetRunnersTeamStrengthScale()
    {
        if (RunnersCount == 0)
            return 0;
        
        float totalScore = 0f;
        foreach (Runner runner in _runners)
        {
            float speedScore = (runner.Speed / runner.MaxSpeed) * .5f;
            float sprintScore = (runner.SprintDuration / runner.MaxSprintDuration) * .5f;
            totalScore += speedScore + sprintScore;
        }
        return totalScore / RunnersCount;
    }

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
}