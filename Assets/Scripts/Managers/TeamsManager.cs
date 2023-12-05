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
    [SerializeField] List<Transform> catchersSpawnPoints;
    [SerializeField, Min(0)] int runnersCount;
    
    List<Catcher> _catchers = new List<Catcher>();
    public int CatchersCount => _catchers.Count;
    List<Runner> _runners = new List<Runner>();
    public int RunnersCount => _runners.Count;
    [HideInInspector] public List<Runner> RunnersNotInSafeArea = new List<Runner>();

    [HideInInspector] public CustomUnityEvent TeamsCountBroadcaster;

    int _catcherSpawnPointIndex;

    public enum Team
    {
        Runner,
        Catcher,
        Nuteral,
        Guard,
    }

    new void Awake()
    {
        base.Awake();
        LoadRunners();
        LoadCatchers();
        TeamsCountBroadcaster = new CustomUnityEvent();
    }

    public void AddRunner(Vector2 position)
    {
        GameObject spawnedRunnerObject = Instantiate(_runnerPrefab, position, Quaternion.identity, runnersParent);
        Runner runner = spawnedRunnerObject.GetComponent<Runner>();
        runner.TeamsManager = this;
        runner.grid = playersGrid;
        runner.PathRequestManager = pathRequestManager;
        _runners.Add(runner);
    }

    void LoadRunners()
    {
        for (int i=0; i < runnersCount; i++)
            AddRunner(GetRandomSafeNode().WorldPosition);
    }

    public void AddCatcher(Vector2 position, Transform spawnPoint = null)
    {
        GameObject spawnedCactherObject = Instantiate(_catcherPrefab, position, Quaternion.identity, catchersParent);
        Catcher catcher = spawnedCactherObject.GetComponent<Catcher>();
        catcher.TeamsManager = this;
        catcher.grid = playersGrid;
        catcher.PathRequestManager = pathRequestManager;
        catcher.SpawnPoint = spawnPoint != null ? spawnPoint : catchersSpawnPoints[_catcherSpawnPointIndex++ % catchersSpawnPoints.Count];
        _catchers.Add(catcher);
    }

    void LoadCatchers()
    {
        foreach (Transform spawnPoint in catchersSpawnPoints)
            AddCatcher(spawnPoint.position, spawnPoint);
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

    public void RemovePlayer(Player player)
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

        List<Player> players = new List<Player>(_catchers);
        players.AddRange(_runners);
        TeamsCountBroadcaster.IsActive = false;

        foreach (Player p in players)
            p.Die();
        
        TeamsCountBroadcaster.IsActive = true;
        TeamsCountBroadcaster.Invoke();
    }
}