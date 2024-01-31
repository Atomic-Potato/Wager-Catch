using System;
using System.Collections;
using System.IO;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

public class Catcher : TeamPlayer
{
    #region Global Variables
    [Space, Header("Catcher Properties")]
    [Space, SerializeField, Min(0)] int _health = 2;
    public int Health => _health;

    [SerializeField, Min(0f)] float _timeToCatch = 0.5f;
    [SerializeField, Min(0f)] float _timeToRecoverCatch = 1.25f;

    [SerializeField, Min(0f)] Vector2 _catchAreaRadiusRange = new Vector2(.25f, 3f);
    public Vector2 CatchAreaRadiusBounds => _catchAreaRadiusRange;
    float _catchAreaRadius = .75f;
    public float MaxCatchAreaRadius => _catchAreaRadiusRange.y;
    public float CatchAreaRadius => _catchAreaRadius;

    [SerializeField] Transform _catchAreaOrigin;

    [Space]
    [SerializeField] SpriteRenderer _catchToolSprite;
    [Tooltip("Where the catch effect will be spawned")]
    [SerializeField] Transform _catchToolEndPoint;
    [SerializeField] GameObject _catchEffect;

    [Space, Header("Bonking")]
    [SerializeField, Min(0f)] float _bonkedTime = 2f;
    public float BonkedTime => _bonkedTime;

    [Space, Header("Catcher Gizmos")]
    [SerializeField] bool _isDrawCatchArea;
    [SerializeField] float _catchAreaDisplayedRadius = .75f;
    [SerializeField] Color _catchAreaColor = new Color(1f, 0f, 0f, 1f);

    [HideInInspector] public Transform SpawnPoint;
    Runner _targetRunner;
    public Runner TargetRunner => _targetRunner;
    public Runner BonkingRunner;
    public bool IsBonkerOnMyAss => BonkingRunner != null;
    bool _isCatchingTarget;
    public bool IsCatchingTarget => _isCatchingTarget;
    Coroutine _catchCoroutine;

    [HideInInspector] public UnityEvent BonkedBroadcaster;
    #endregion

    new void Awake()
    {
        base.Awake();

        BonkEndBroadcaster = new UnityEvent();

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
        if (_catchCoroutine == null && !_isSleeping && _isTargetWithinCatchRange)
            _catchCoroutine = StartCoroutine(Catch());
        CorrectGunPointSide();
        if (_isSleeping)
            _catchToolSprite.enabled = false;
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
        EnableCatchingTool();
        yield return new WaitForSeconds(_timeToCatch);

        if (_isTargetWithinCatchRange && !_isSleeping)
        {
            CatchTarget();
            AddCatchBonusToBalance();
        }
        DisableCatchingTool();

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
        void CatchTarget()
        {
            PlayCatchEffect();
            _targetRunner.Die();
            _isCatchingTarget = false;
        }
        void AddCatchBonusToBalance()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager.PlayerTeam_TEAM_TAG == TagsManager.TeamTag.Catcher)
                gameManager.AddBalance(gameManager.CatchingBonus);
        }
        void EnableCatchingTool()
        {
            _catchToolSprite.enabled = true;
            SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.GunCock, true);
        }
        void DisableCatchingTool()
        {
            _catchToolSprite.enabled = false;
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
        {
            if (TeamsManager.RunnersCount == 0)
                return;
            FindRunnerTarget();
        }

        if (_targetRunner == null || _targetRunner.IsInSafeArea)
        {
            Runner newTarget = GetClosestAvailableNotInSafeAreaRunner();
            if (newTarget == null)
                _target = SpawnPoint.position;
            else
            {
                _targetRunner.Catchers.Remove(this);
                _targetRunner = newTarget;
                _targetRunner.Catchers.Add(this);
                
                _target = _targetRunner.transform.position;
            }
        }
        else
            _target = _targetRunner.transform.position;

        if (!_isPathRequestSent)
            SendPathRequest();
    }

    void FindRunnerTarget()
    {
        _isCatchingTarget = false;

        if (_targetRunner != null)
            _targetRunner.Catchers.Remove(this);

        _targetRunner = GetClosestAvailableNotInSafeAreaRunner();
        if (_targetRunner == null)
            _targetRunner = GetClosestAvailableInSafeAreaRunner();
        if (_targetRunner == null)
            _targetRunner = GetClosestNotInSafeAreaRunner();
        if (_targetRunner == null)
            _targetRunner = GetClosestRunner();

        if (_targetRunner != null)
        {
            _targetRunner.Catchers.Add(this);
            _isCatchingTarget = true;
        }
        
        
        Runner GetClosestAvailableInSafeAreaRunner()
        {
            float? minDistance = null;
            Runner closestRunner = null;
            foreach (Runner runner in TeamsManager.Instance.Runners)
            {
                if (!runner.IsInSafeArea || runner.Catchers.Count != 0)
                    continue;

                float distance = Vector2.Distance(transform.position, runner.transform.position);
                if (minDistance == null || distance < minDistance)
                {
                    minDistance = distance;
                    closestRunner = runner;
                }
            }

            return closestRunner;
        }
        Runner GetClosestNotInSafeAreaRunner()
        {
            if (TeamsManager.RunnersNotInSafeArea.Count == 0)
                return null;
            
            float? minDistance = null;
            Runner closestRunner = null;
            foreach (Runner runner in TeamsManager.Instance.RunnersNotInSafeArea)
            {
                float distance = Vector2.Distance(transform.position, runner.transform.position);
                if (minDistance == null || distance < minDistance)
                {
                    minDistance = distance;
                    closestRunner = runner;
                }
            }

            return closestRunner;
        }
        Runner GetClosestRunner()
        {
            float? minDistance = null;
            Runner closestRunner = null;
            foreach (Runner runner in TeamsManager.Instance.Runners)
            {
                float distance = Vector2.Distance(transform.position, runner.transform.position);
                if (minDistance == null || distance < minDistance)
                {
                    minDistance = distance;
                    closestRunner = runner;
                }
            }

            return closestRunner;
        }
    }

    Runner GetClosestAvailableNotInSafeAreaRunner()
    {
        if (TeamsManager.RunnersNotInSafeArea.Count == 0)
            return null;
        
        float? minDistance = null;
        Runner closestRunner = null;
        foreach (Runner runner in TeamsManager.Instance.RunnersNotInSafeArea)
        {
            if (runner.Catchers.Count != 0)
                continue;

            float distance = Vector2.Distance(transform.position, runner.transform.position);
            if (minDistance == null || distance < minDistance)
            {
                minDistance = distance;
                closestRunner = runner;
            }
        }
        return closestRunner;
    }
    
    #endregion
    public void BonkSelf()
    {
        BonkedBroadcaster.Invoke();
        DeductHealthPoint();
        if (_health <= 0)
            return;

        Sleep(_bonkedTime);
    }

    public void DeductHealthPoint()
    {
        _health--;
        if (_health <= 0)
            Die();
    }

    public override void Die()
    {
        TeamsManager.RemoveCatcher(this);
        base.Die();
    }
}
