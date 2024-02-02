using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pathfinding;
using UnityEngine;

public class Runner : TeamPlayer
{
    #region Global Variables
    [Space, Header("Runner Properites")]
    [SerializeField, Min(0f)] Vector2 randomTimeRangeToStartRunning = new Vector2(.25f, 1f);

    [Space, Header("Panik")]
    [SerializeField, Min(0f)] float _catcherDetectionRange = 1.5f;

    [Space,Header ("Bonking")]
    [SerializeField, Min(0f)] float _bonkingRange = .75f;
    public float BonkingRange => _bonkingRange;
    Catcher _catcherToBonk;
    public Catcher CatcherToBonk => _catcherToBonk;

    
    [Space, Header("Objective")]
    [SerializeField] Objective _defaultObjective = Objective.Hide; // Best kept at Hide, else it may make a stack overflow exception when starting objective
    Objective _currentObjective;
    public Objective CurrentObjective => _currentObjective;
    public enum Objective
    {
        None,
        Bonk,
        Hide,
    }
    int _objectivesCount => Enum.GetValues(typeof(Objective)).Length;
    
    [Space, Header("Debugging")]
    [SerializeField] bool _isDrawBonkingRange;

    bool _isInSafeArea;
    public bool IsInSafeArea => _isInSafeArea;
    public List<Catcher> Catchers = new List<Catcher>(); 
    #endregion

    new void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (!_isGizmosEnabled)
            return;
        if (_isDrawBonkingRange)
            Gizmos.DrawWireSphere(transform.position, _bonkingRange);
    }

    void Start()
    {
        TeamsManager.RunnersNotInSafeArea.Add(this);
        SetNewObjective();
    }

    new void Update()
    {
        base.Update();

        ExecuteCurrentObjective();

        if (IsCatchersInProximity())
            Panik();
        else
            Kalm();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == TagsManager.Tag.SafeArea.ToString())
        {
            _isInSafeArea = true;
            RemoveRunnerFromTheField();
            AddEscapeBonusToBalance();
        }

        void AddEscapeBonusToBalance()
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager.PlayerTeam_TEAM_TAG == TagsManager.TeamTag.Runner)
                gameManager.AddBalance(gameManager.EscapingBonus);
        }
        void RemoveRunnerFromTheField()
        {
            if (TeamsManager.RunnersNotInSafeArea.Contains(this))
                TeamsManager.RunnersNotInSafeArea.Remove(this);
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

    

    void RequestPathToTarget()
    {
        if (!_isPathRequestSent)
        {
            SendPathRequest();
        }
    }

    public override void Die()
    {
        EndBonk();
        TeamsManager.RunnersNotInSafeArea.Remove(this);
        TeamsManager.RemoveRunner(this);
        base.Die();
    }

    bool IsCatchersInProximity()
    {
        if (Catchers.Count == 0)
            return false;

        foreach (Catcher catcher in Catchers)
        {
            float distanceToCatcher = Vector2.Distance((Vector2)transform.position, (Vector2)catcher.transform.position);
            if (distanceToCatcher <= _catcherDetectionRange)
                return true;
        }
        return false;
    }

    #region Objectives
    void ExecuteCurrentObjective()
    {
        switch (_currentObjective)
        {
            case Objective.Bonk:
                ExecuteBonking();
                break;
            case Objective.Hide:
                ExecuteHiding();
                break;
            default:
                return;
        }
    }

    Coroutine _delayNewObjectiveCoroutine;
    void SetNewObjective(bool isDelayed = false)
    {
        Objective objective = GetRandomObjective();
        if (!isDelayed)
            StartObjective(objective);
        else
        {
            if(_delayNewObjectiveCoroutine == null)
                _delayNewObjectiveCoroutine = StartCoroutine(DelayNewObjective());
        }

        IEnumerator DelayNewObjective()
        {
            float delayTime = _isCollided ? 0f : UnityEngine.Random.Range(randomTimeRangeToStartRunning.x, randomTimeRangeToStartRunning.y);
            yield return new WaitForSeconds(delayTime);
            StartObjective(objective);
            _delayNewObjectiveCoroutine = null;
        }
    }

    Objective GetRandomObjective()
    {
        return Objective.Bonk;
        int _objective = UnityEngine.Random.Range(0, _objectivesCount);
        return (Objective)_objective;
    }
    
    void StartObjective(Objective objective)
    {
        bool isObjectiveStarted;
        _currentObjective = objective;
        switch (objective)
        {
            case Objective.Bonk:
                isObjectiveStarted = StartBonking(); 
                break;
            case Objective.Hide:
                isObjectiveStarted = StartHiding();
                break;
            default:
                isObjectiveStarted = false;
                break;
        }

        if (!isObjectiveStarted)
        {
            _currentObjective = _defaultObjective;
            StartObjective(_defaultObjective);
        }

    }
    #region Bonking 
    bool StartBonking()
    {
        _catcherToBonk = GetRandomAvaliableCatcher();
        if (_catcherToBonk == null)
            return false;
        _catcherToBonk.BonkingRunner = this;
        _target = _catcherToBonk.transform.position;
        RequestPathToTarget();
        return true;

        Catcher GetRandomAvaliableCatcher()
        {
            if (TeamsManager.Catchers.Count == 0)
                return null;
            foreach (Catcher catcher in TeamsManager.Instance.Catchers)
            {
                if (catcher.TargetRunner != this && catcher.BonkingRunner == null && !catcher.IsSleeping)
                    return catcher;
            }
            return null;        
        }
    }

    bool _isBonking;
    public bool IsBonking => _isBonking;
    void ExecuteBonking()
    {
        if (_catcherToBonk == null || _catcherToBonk.TargetRunner == this)
        {
            EndBonk();
            return;
        }

        _target = _catcherToBonk.transform.position;
        RequestPathToTarget();

        if (IsWithinBonkingRange() && !_isBonking)
        {
            _isBonking = true;
            BonkStartBroadcaster.Invoke();
        }

        bool IsWithinBonkingRange()
        {
            float distanceToCatcher = Vector2.Distance(transform.position, _catcherToBonk.transform.position);
            return distanceToCatcher <= _bonkingRange;
        }

    }

    // This function is to be executed from the bonking animation
    public override void Bonk()
    {
        base.Bonk();
        if (_catcherToBonk == null)
            return;
        _catcherToBonk.BonkSelf();
    }

    // This function is to be executeed at the end of the bonking animation
    public override void EndBonk()
    {
        if (_catcherToBonk == null)
            return;

        base.Bonk();

        _isBonking = false;
        _currentObjective = Objective.None;
        _catcherToBonk.BonkingRunner = null;
        StartObjective(Objective.Hide);
        BonkEndBroadcaster.Invoke();
    }
    #endregion
    
    #region Hiding
    bool StartHiding()
    {
        _target = TeamsManager.GetRandomSafeNode()?.WorldPosition;
        ForceSendPathRequest();
        return true;
    }

    void ExecuteHiding()
    {
        if (_isReachedDestination)
        {
            FinishHiding();
            SetNewObjective(true);
        }
    }

    void FinishHiding()
    {
        _currentObjective = Objective.None;
    }
    #endregion
    #endregion
}
