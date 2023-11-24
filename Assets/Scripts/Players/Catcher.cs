using System.Collections;
using Pathfinding;
using UnityEngine;

public class Catcher : Player
{
    [Space, Header("Catcher Properties")]
    [SerializeField, Min(0f)] float _timeToCatch = 0.5f;
    [SerializeField, Min(0f)] float _timeToRecoverCatch = 1.25f;
    [SerializeField, Min(0f)] float _catchAreaRadius = .75f;
    [SerializeField] Transform _catchAreaOrigin;

    [Space]
    [SerializeField] SpriteRenderer _catchToolSprite;
    [Tooltip("Where the catch effect will be spawned")]
    [SerializeField] Transform _catchToolEndPoint;
    [SerializeField] GameObject _catchEffect;

    [Space, Header("Catcher Gizmos")]
    [SerializeField] bool _isDrawCatchArea;
    [SerializeField] Color _catchAreaColor = new Color(1f, 0f, 0f, 1f);

    [HideInInspector] public Transform SpawnPoint;
    Runner _targetRunner;
    bool _isCatchingTarget;
    Coroutine _catchCoroutine;

    new void Awake()
    {
        base.Awake();

        if (_catchToolSprite.enabled)
            _catchToolSprite.enabled = false;
    }

    new void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (_isDrawCatchArea)
        {
            Vector3 origin = _catchAreaOrigin != null ? _catchAreaOrigin.position : transform.position;
            Gizmos.color = _catchAreaColor;
            Gizmos.DrawWireSphere(origin, _catchAreaRadius);
        }
    }

    new void Update()
    {
        base.Update();
        Pathfind();
        if (_catchCoroutine == null && _isTargetWithinCatchRange)
            _catchCoroutine = StartCoroutine(Catch());
    }

    #region Catching
    bool _isTargetWithinCatchRange
    {
        get{
            if (_targetRunner == null || _targetRunner.IsInSafeArea)
                return false;
            Vector3 origin = _catchAreaOrigin != null ? _catchAreaOrigin.position : transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, _catchAreaRadius);
            foreach(Collider2D collider in colliders)
            {
                if (collider.gameObject == _targetRunner.gameObject)
                    return true;
            }
            return false;
        }
    }

    IEnumerator Catch()
    {
        _catchToolSprite.enabled = true;
        yield return new WaitForSeconds(_timeToCatch);

        if (_isTargetWithinCatchRange)
        {
            if (_catchEffect != null)
                Instantiate(_catchEffect, _catchToolEndPoint.position, Quaternion.identity);
            KillTarget();
        }
        _catchToolSprite.enabled = false;

        yield return new WaitForSeconds(_timeToRecoverCatch);
        _catchCoroutine = null;

        void KillTarget()
        {
            TeamsManager.RunnersNotInSafeArea.Remove(_targetRunner);
            _targetRunner.Catchers.Clear();
            _targetRunner.Die();
            _targetRunner = null;
            FindRunnerTarget();
        }
    }
    #endregion

    #region Pathfinding to Target
    void Pathfind()
    {
        if (!_isCatchingTarget)
            FindRunnerTarget();
        if (_targetRunner != null && _targetRunner.IsInSafeArea)
            FindRunnerTarget();
     
        if (_targetRunner != null)
            _target = _targetRunner.transform.position;
        else
            _target = SpawnPoint.position;

        if (!_isPathRequestSent)
            SendPathRequest();
    }

    void FindRunnerTarget()
    {
        _isCatchingTarget = false;
        
        if (TeamsManager.RunnersNotInSafeArea.Count == 0)
        {
            _targetRunner = null;
            return;
        }

        Runner target = null;
        Runner closestRunner = null;
        float distanceToClosestRunner = 0f;

        foreach(Runner runner in TeamsManager.RunnersNotInSafeArea)
        {
            UpdateClosestRunner(runner);

            if (runner.Catchers.Count == 0)
            {
                target = runner;
                break;
            }
        }

        if (target == null)
            target = closestRunner;

        if (target != null)
        {
            target.Catchers.Add(this);
            _isCatchingTarget = true;
        }

        _targetRunner = target;


        void UpdateClosestRunner(Runner runner)
        {
            if (closestRunner == null)
            {
                closestRunner = runner;
                distanceToClosestRunner = Vector2.Distance(transform.position, runner.transform.position);
            }
            else
            {
                float distanceToRunner = Vector2.Distance(transform.position, runner.transform.position);
                if (distanceToRunner < distanceToClosestRunner)
                {
                    closestRunner = runner;
                    distanceToClosestRunner = distanceToRunner;
                }
            }
        }
        #endregion
    }
}
