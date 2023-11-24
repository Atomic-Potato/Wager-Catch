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
    public bool IsInSafeArea => _isInSafeArea;
    public List<Catcher> Catchers = new List<Catcher>(); 
    Coroutine _delayNextRequestCoroutine;

    void Start()
    {
        RequestPathToTarget();
    }

    new void Update()
    {
        base.Update();

        if (_isReachedDestination && !_isPathRequestSent && _delayNextRequestCoroutine == null)
            _delayNextRequestCoroutine = StartCoroutine(DelayNextPathRequest());
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Safe Area")
        {
            _isInSafeArea = true;
            if (TeamsManager.RunnersNotInSafeArea.Contains(this))
                TeamsManager.RunnersNotInSafeArea.Remove(this);
            Catchers.Clear();
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Safe Area")
        {
            _isInSafeArea = false;
            if (!TeamsManager.RunnersNotInSafeArea.Contains(this))
                TeamsManager.RunnersNotInSafeArea.Add(this);
        }
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
