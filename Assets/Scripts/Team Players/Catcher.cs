using System.Collections;
using System.IO;
using Pathfinding;
using UnityEngine;

public class Catcher : TeamPlayer
{
    #region Global Variables
    [Space, Header("Catcher Properties")]
    [SerializeField, Min(0f)] float _timeToCatch = 0.5f;
    [SerializeField, Min(0f)] float _timeToRecoverCatch = 1.25f;

    [SerializeField, Min(0f)] Vector2 _catchAreaRadiusRange = new Vector2(.25f, 3f);
    float _catchAreaRadius = .75f;
    public float MaxCatchAreaRadius => _catchAreaRadiusRange.y;
    public float CatchAreaRadius => _catchAreaRadius;

    [SerializeField] Transform _catchAreaOrigin;

    [Space]
    [SerializeField] SpriteRenderer _catchToolSprite;
    [Tooltip("Where the catch effect will be spawned")]
    [SerializeField] Transform _catchToolEndPoint;
    [SerializeField] GameObject _catchEffect;

    [Space, Header("Catcher Gizmos")]
    [SerializeField] bool _isDrawCatchArea;
    [SerializeField] float _catchAreaDisplayedRadius = .75f;
    [SerializeField] Color _catchAreaColor = new Color(1f, 0f, 0f, 1f);

    [HideInInspector] public Transform SpawnPoint;
    Runner _targetRunner;
    bool _isCatchingTarget;
    Coroutine _catchCoroutine;
    #endregion

    new void Awake()
    {
        base.Awake();

        if (_catchToolSprite.enabled)
            _catchToolSprite.enabled = false;
        RandomizeValues();

        void RandomizeValues()
        {
            _catchAreaRadius = UnityEngine.Random.Range(_catchAreaRadiusRange.x, _catchAreaRadiusRange.y);
        }
    }

    new void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (_isDrawCatchArea)
        {
            Vector3 origin = _catchAreaOrigin != null ? _catchAreaOrigin.position : transform.position;
            Gizmos.color = _catchAreaColor;
            Gizmos.DrawWireSphere(origin, _catchAreaDisplayedRadius);
        }
    }

    new void Update()
    {
        base.Update();
        Pathfind();
        if (_catchCoroutine == null && _isTargetWithinCatchRange)
            _catchCoroutine = StartCoroutine(Catch());
        CorrectGunPointSide();
    }

    void CorrectGunPointSide()
    {
        if (_targetRunner == null && !_catchToolSprite.enabled)
            return;
        
        if (_targetRunner.transform.position.x < transform.position.x)
        {
            if (!_catchToolSprite.flipX)
            {
                _catchToolSprite.transform.localPosition = new Vector3 (-_catchToolSprite.transform.localPosition.x, _catchToolSprite.transform.localPosition.y, _catchToolSprite.transform.localPosition.z);
                _catchToolSprite.flipX = true;
            }
        }
        else
        {
            if (_catchToolSprite.flipX)
            {
                _catchToolSprite.transform.localPosition = new Vector3 (-_catchToolSprite.transform.localPosition.x, _catchToolSprite.transform.localPosition.y, _catchToolSprite.transform.localPosition.z);
                _catchToolSprite.flipX = false;
            }
        }
    }

    int GetTargetXDirection()
    {
        if (_targetRunner == null)
            return 0;

        return  _targetRunner.transform.position.x < transform.position.x ? -1 : 1;
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
        SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.GunCock, true);
        yield return new WaitForSeconds(_timeToCatch);

        if (_isTargetWithinCatchRange)
        {
            PlayCatchEffect();
            _targetRunner.Die();
        }
        _catchToolSprite.enabled = false;

        yield return new WaitForSeconds(_timeToRecoverCatch);
        _catchCoroutine = null;

        void PlayCatchEffect()
        {
            if (_catchEffect == null)
                return;

            int direction = GetTargetXDirection();
            Quaternion rotation =  Quaternion.Euler(new Vector3(0f, direction * 90f, 0f));
            Instantiate(_catchEffect, _catchToolEndPoint.position, rotation);

            SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.GunBoom, true);
        }
    }

    public void RemoveTarget()
    {
        _targetRunner = null;
        FindRunnerTarget();
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
    }
    #endregion

    public override void Die()
    {
        TeamsManager.RemoveCatcher(this);
        base.Die();
    }
}
