﻿using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using TMPro;
using UnityEngine;

public class Runner : TeamPlayer
{
    [Space, Header("Runner Properites")]
    [SerializeField, Min(0f)] Vector2 randomTimeRangeToStartRunning = new Vector2(.25f, 1f);

    [Space, Header("Panik")]
    [SerializeField, Min(0f)] float _catcherDetectionRange = 1.5f;
    [SerializeField] GameObject _panikMarker;
    [SerializeField] TMP_Text _debuggingText;

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
        base.Die();
    }

    bool IsCatchersInProximity()
    {
        if (Catchers.Count == 0)
            return false;

        foreach (Catcher catcher in Catchers)
        {
            float distanceToCatcher = Vector2.Distance(transform.position, catcher.transform.position);
            if (distanceToCatcher <= _catcherDetectionRange)
                return true;
        }
        return false;
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
                float length = SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.Scream);
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
