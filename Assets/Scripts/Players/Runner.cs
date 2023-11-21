using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Runner : Player
{
    [Space, Header("Runner Properites")]
    [SerializeField, Min(0f)] Vector2 randomTimeRangeToStartRunning = new Vector2(.25f, 1f);
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] LayerMask safeAreaMask;

    bool _isInSafeArea;
    public bool IsInSafeZone => _isInSafeArea;
    public List<Catcher> Catchers = new List<Catcher>(); 
    Coroutine _delayNextRequestCoroutine;

    void Start()
    {
        RequestPathToTarget();
    }

    void Update()
    {
        if (_isReachedDestination && !_isPathRequestSent && _delayNextRequestCoroutine == null)
            _delayNextRequestCoroutine = StartCoroutine(DelayNextPathRequest());

        _isInSafeArea = Physics2D.OverlapBox(transform.position + (Vector3)boxCollider.offset, boxCollider.size, 0f, safeAreaMask);
        if (_isInSafeArea && TeamsManager.RunnersNotInSafeArea.Contains(this))
            TeamsManager.RunnersNotInSafeArea.Remove(this);
        else if (!TeamsManager.RunnersNotInSafeArea.Contains(this))
            TeamsManager.RunnersNotInSafeArea.Add(this);
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
