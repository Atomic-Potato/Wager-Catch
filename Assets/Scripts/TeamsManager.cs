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
    
    List<Catcher> _catchers = new List<Catcher>();
    List<Runner> _runners = new List<Runner>();

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
            Runner runner = spawnedRunnerObject.GetComponent<Runner>();
            runner.PathRequestManager = pathRequestManager;
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
            Catcher catcher = spawnedCactherObject.GetComponent<Catcher>();
            catcher.PathRequestManager = pathRequestManager;
            catcher.spawnPoint = spawnPoint;
            _catchers.Add(catcher);
        }
    }
}
