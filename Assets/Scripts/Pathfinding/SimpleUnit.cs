using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class SimpleUnit : UnitBase
    {
        [Space, Header("Target")]
        [SerializeField] Transform _targetTransform;

        [Space, Header("Randomized Target")]
        [SerializeField] bool _isTargetRandomized;
        [SerializeField, Min(0f)] Vector2 _timeRangeToChooseNewTarget = new Vector2(1f, 2f);
        [SerializeField] List<Transform> _targetsList;

        [Space, Header("Other properties")]
        [SerializeField] bool _isTargetMoving;

        new void Start()
        {
            base.Start();
            _target = GetTargetPosition();
            SendPathRequest();
        }

        new void Update()
        {
            base.Update();
            if (!_isTargetMoving && _isReachedDestination)
                SendDelayedPathRequest();
            else if (_isTargetMoving)
            {
                _target = GetTargetPosition();
                if (!_isPathRequestSent)
                    SendPathRequest();
            }

        }

        Coroutine _delayedPahtRequestCoroutine;
        void SendDelayedPathRequest()
        {
            if (_delayedPahtRequestCoroutine == null)
                _delayedPahtRequestCoroutine = StartCoroutine(DelayedPathRequest());
            IEnumerator DelayedPathRequest()
            {
                yield return new WaitForSeconds(Random.Range(_timeRangeToChooseNewTarget.x, _timeRangeToChooseNewTarget.y));
                _target = GetTargetPosition();
                if (!_isPathRequestSent)
                    SendPathRequest();
                _delayedPahtRequestCoroutine = null;
            }
        }

        Vector2 GetTargetPosition()
        {
            if (!_isTargetRandomized)
                return _targetTransform.position;
            else
                return _targetsList[UnityEngine.Random.Range(0, _targetsList.Count)].position;
        }
    }
}
