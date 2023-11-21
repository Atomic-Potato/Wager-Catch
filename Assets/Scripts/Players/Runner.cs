using System.Collections;
using Pathfinding;
using UnityEngine;

public class Runner : Player
{
    [Space, Header("Runner Properites")]
    [SerializeField, Min(0f)] Vector2 randomTimeRangeToStartRunning = new Vector2(.25f, 1f);

    Coroutine _delayNextRequestCoroutine;

    void Start()
    {
        RequestPathToTarget();
    }

    void Update()
    {
        if (_isReachedDestination && !_isPathRequestSent && _delayNextRequestCoroutine == null)
            _delayNextRequestCoroutine = StartCoroutine(DelayNextPathRequest());
    }

    IEnumerator DelayNextPathRequest()
    {
        float delayTime = Random.Range(randomTimeRangeToStartRunning.x, randomTimeRangeToStartRunning.y);
        yield return new WaitForSeconds(delayTime);
        RequestPathToTarget();
        _delayNextRequestCoroutine = null;
    }

    void RequestPathToTarget()
    {
        _target = TeamsManager.GetRandomSafeNode()?.WorldPosition;
        SendPathRequest();
    }
}
