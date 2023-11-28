using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Events;

public class TeamsManager : Singleton<TeamsManager>
{
    [SerializeField] GameObject catcherPrefab;
    [SerializeField] GameObject runnerPrefab;
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

    [HideInInspector] public UnityEvent TeamsCountBroadcaster;

    new void Awake()
    {
        base.Awake();
        LoadRunners();
        LoadCatchers();
        TeamsCountBroadcaster = new UnityEvent();
    }

    void LoadRunners()
    {
        for (int i=0; i < runnersCount; i++)
        {
            GameObject spawnedRunnerObject = Instantiate(runnerPrefab, GetRandomSafeNode().WorldPosition, Quaternion.identity, runnersParent);
            Runner runner = spawnedRunnerObject.GetComponent<Runner>();
            runner.TeamsManager = this;
            runner.grid = playersGrid;
            runner.PathRequestManager = pathRequestManager;
            _runners.Add(runner);
        }
    }

    void LoadCatchers()
    {
        foreach (Transform spawnPoint in catchersSpawnPoints)
        {
            GameObject spawnedCactherObject = Instantiate(catcherPrefab, spawnPoint.position, Quaternion.identity, catchersParent);
            Catcher catcher = spawnedCactherObject.GetComponent<Catcher>();
            catcher.TeamsManager = this;
            catcher.grid = playersGrid;
            catcher.PathRequestManager = pathRequestManager;
            catcher.SpawnPoint = spawnPoint;
            _catchers.Add(catcher);
        }
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
}
