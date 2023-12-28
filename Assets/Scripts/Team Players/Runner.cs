using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

public class Runner : TeamPlayer
{
    [Space, Header("Runner Properites")]
    [SerializeField, Min(0f)] Vector2 randomTimeRangeToStartRunning = new Vector2(.25f, 1f);

    [Space, Header("Panik")]
    [SerializeField, Min(0f)] float _catcherDetectionRange = 1.5f;
    [SerializeField] GameObject _panikMarker;

    bool _isInSafeArea;
    public bool IsInSafeArea => _isInSafeArea;
    public List<Catcher> Catchers = new List<Catcher>(); 
    Coroutine _delayNextRequestCoroutine;
    Coroutine _screamCoroutine;

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
        
        if (GetCatchersInProximity().Count > 0)
            Panik();
        else
            Kalm();
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

    List<RaycastHit2D> GetCatchersInProximity()
    {
        List<RaycastHit2D> hits = Physics2D.CircleCastAll(transform.position, _catcherDetectionRange, Vector2.zero).ToList<RaycastHit2D>();
        List<RaycastHit2D> catchers = new List<RaycastHit2D>();

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.tag == TagsManager.Tag.Catcher.ToString())
            {
                Debug.Log(hit.collider.gameObject.tag);
                catchers.Add(hit);
            }
        }

        return catchers;
    }

    void Panik()
    {
        PlayScreamSound();
        DisplayPanikIndicator();

        void DisplayPanikIndicator()
        {
            _panikMarker.SetActive(true);
        }

        void PlayScreamSound()
        {
            if (_screamCoroutine == null)
                _screamCoroutine = StartCoroutine(Scream());

            IEnumerator Scream()
            {
                float length = SoundManager.Instance.PlayRandomScreamAtPosition(transform.position);
                yield return new WaitForSeconds(length);
                _screamCoroutine = null;
            }
        }
    }

    void Kalm()
    {
        HidePanikIndicator();

        void HidePanikIndicator()
        {
            _panikMarker.SetActive(false);
        }
    }

}
