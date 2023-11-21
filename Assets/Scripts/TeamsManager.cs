using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Runtime.InteropServices;

public class TeamsManager : MonoBehaviour
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
    
    List<Player> _catchers = new List<Player>();
    List<Player> _runners = new List<Player>();

    void Awake()
    {
        LoadRunners();
        LoadCatchers();
    }

    void LoadRunners()
    {
        for (int i=0; i < runnersCount; i++)
        {
            GameObject spawnedRunnerObject = Instantiate(runnerPrefab, GetRandomSafeNode().WorldPosition, Quaternion.identity, runnersParent);
            Player runner = spawnedRunnerObject.GetComponent<Player>();
            runner.PathRequestManager = pathRequestManager;
            runner.IsRunner = true;
            _runners.Add(runner);
        }

        Node GetRandomSafeNode()
        {
            int n = Random.Range(0, playersGrid.SafeNodes.Count);
            return playersGrid.SafeNodes[n];
        }
    }

    void LoadCatchers()
    {
        foreach (Transform spawnPoint in catchersSpawnPoints)
        {
            GameObject spawnedCactherObject = Instantiate(catcherPrefab, spawnPoint.position, Quaternion.identity, catchersParent);
            Player catcher = spawnedCactherObject.GetComponent<Player>();
            catcher.IsCatcher = true;
            catcher.CatcherSpawnPoint = spawnPoint;
            _catchers.Add(catcher);
        }
    }
}
