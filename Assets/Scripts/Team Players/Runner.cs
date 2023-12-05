using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Runner : TeamPlayer
{
    [Space, Header("Runner Properites")]
    [SerializeField, Min(0f)] Vector2 randomTimeRangeToStartRunning = new Vector2(.25f, 1f);

    bool _isInSafeArea;
    public bool IsInSafeArea => _isInSafeArea;
    public List<Catcher> Catchers = new List<Catcher>(); 
    Coroutine _delayNextRequestCoroutine;

    void Start()
    {
        TeamsManager.RunnersNotInSafeArea.Add(this);
        RequestPathToNewSafeArea();
    }

    new void Update()
    {
        base.Update();

        if (_isReachedDestination && !_isPathRequestSent && _delayNextRequestCoroutine == null)
            _delayNextRequestCoroutine = StartCoroutine(DelayNextPathRequest());
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == TagsManager.Tag.SafeArea.ToString())
        {
            _isInSafeArea = true;
            if (TeamsManager.RunnersNotInSafeArea.Contains(this))
                TeamsManager.RunnersNotInSafeArea.Remove(this);
            Catchers.Clear();
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == TagsManager.Tag.SafeArea.ToString())
        {
            _isInSafeArea = false;
            if (!TeamsManager.RunnersNotInSafeArea.Contains(this))
                TeamsManager.RunnersNotInSafeArea.Add(this);
        }
    }

    IEnumerator DelayNextPathRequest()
    {
        float delayTime = _isCollided ? 0f : Random.Range(randomTimeRangeToStartRunning.x, randomTimeRangeToStartRunning.y);
        yield return new WaitForSeconds(delayTime);
        RequestPathToNewSafeArea();
        _delayNextRequestCoroutine = null;
    }

    void RequestPathToNewSafeArea()
    {
        _target = TeamsManager.GetRandomSafeNode()?.WorldPosition;
        SendPathRequest();
    }

    public override void Die()
    {
        TeamsManager.RunnersNotInSafeArea.Remove(this);
        TeamsManager.RemoveRunner(this);
        foreach(Catcher catcher in Catchers)
            catcher.RemoveTarget();
        Catchers.Clear();
        base.Die();
    }
}
