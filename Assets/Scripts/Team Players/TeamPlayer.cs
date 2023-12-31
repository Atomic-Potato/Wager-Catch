﻿using System.Collections;
using Ability;
using UnityEngine;

namespace Pathfinding
{
    public class TeamPlayer : MonoBehaviour, ITeamPlayer
    {
        #region Global Variables
        [SerializeField, Min(0f)] Vector2 _speedRange = new Vector2(1f, 5f);
        float _speed = 1f;
        public float MaxSpeed => _speedRange.y;
        public float Speed => _speed;

        [Space]
        [SerializeField, Min(0f)] bool _isCanSprint;
        public bool IsCanSprint => _isCanSprint;
        [SerializeField, Min(0f)] float _sprintSpeedMultiplier = 1.75f;
        [SerializeField, Min(0f)] Vector2 _sprintDurationRange = new Vector2(0.5f, 4f);
        float _sprintDuration = 1.75f;
        public float MaxSprintDuration => _sprintDurationRange.y;
        public float SprintDuration => _sprintDuration;
        [SerializeField, Min(0f)] float _sprintRecoveryMultiplier = 2f;
        
        [Space]
        [SerializeField] Vector2 _collisionCheckSize = Vector2.one;
        [SerializeField] Vector2 _collisionCheckOffset;
        
        [Space]
        [SerializeField] LayerMask _collisionLayer;
        [SerializeField] LayerMask _slowdownAreaLayer;
        
        [Space]
        [SerializeField] GameObject _deathEffect;

        [Space, Header("Gizmos")]
        [SerializeField] bool _isDrawCollisionCheckSize;
        [SerializeField] Color _collisionCheckCollor = Color.red;
        [SerializeField] bool isDrawPath;
        [SerializeField] Color pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool isRandomPathColor = true;

        [HideInInspector] public Grid grid;
        [HideInInspector] public PathRequestManager PathRequestManager;
        [HideInInspector] public TeamsManager TeamsManager;

        protected Vector2? _target;
        Vector2 _facingDirection  = Vector2.down; 
        public Vector2 FacingDirection => _facingDirection;
        Vector2? _previousPosition;
        bool _isMoving;
        public bool IsMoving => _isMoving;

        Vector2[] _pathToTarget;
        Coroutine _followPathCoroutine;
        int _pathIndex;
        Vector2? _currentWaypoint = null;
        Node _endNodeCache = null;
        Node _startNodeCache = null;
        protected bool _isPathRequestSent;
        protected bool _isReachedDestination;
        protected bool _isStopFollowingPath;
        protected bool _isCollided;
        protected bool _isSlowedDown;
        protected bool _isSleeping;
        public bool IsSleeping => _isSleeping;
        protected bool _isSprinting;
        public bool IsSprinting => _isSprinting;
        protected bool _isRecoveringSprint;

        float _appliedSpeed;
        float _sprintTimer;
        float _currentSleepTime;
        public float CurrentSleepTime => _currentSleepTime;

        Coroutine _wakeCoroutine;

        CustomUnityEvent _sleepEvent;
        public CustomUnityEvent SleepEvent => _sleepEvent;
        #endregion

        protected void OnDrawGizmos()
        {
            if(isDrawPath)
            {
                if (_pathToTarget != null)
                {
                    for(int i = _pathIndex; i < _pathToTarget.Length; i++)
                    {
                        Gizmos.color = pathColor;
                        if(i != _pathIndex)
                            Gizmos.DrawLine(_pathToTarget[i-1], _pathToTarget[i]);
                        Gizmos.DrawCube(_pathToTarget[i], new Vector3(.25f, .25f, 0f));
                    }
                }
            }

            if (_isDrawCollisionCheckSize)
            {
                Gizmos.color = _collisionCheckCollor;
                Gizmos.DrawWireCube(transform.position + (Vector3)_collisionCheckOffset, _collisionCheckSize);
            }
        }

        protected void Awake()
        {
#if UNITY_EDITOR
            if (isRandomPathColor)
                pathColor = new Color(Random.Range(.8f,1f), Random.Range(.4f,8f), Random.Range(0f,4f), 1f);
#endif
            RandomizeValues();
            _sleepEvent = new CustomUnityEvent();
            _appliedSpeed = _speed;
            _sprintTimer = _sprintDuration;

            void RandomizeValues()
            {
                _speed = UnityEngine.Random.Range(_speedRange.x, _speedRange.y);
                _sprintDuration = UnityEngine.Random.Range(_sprintDurationRange.x, _sprintDurationRange.y);
            }
        }

        protected void Update()
        {
            UpdateFacingDirection();
            CheckForCollisions();
            CheckForSlowdownAreas();
            if (_isCanSprint)
                Sprint();
        }

        
        protected void SendPathRequest()
        {
            Vector2? targetPosition = (Vector2)_target;
            if (targetPosition == null || _isSleeping)
                return;
            
            PathRequestManager.RequestPath(transform.position, (Vector2)targetPosition, _endNodeCache, _startNodeCache, UpdatePath);
            _isPathRequestSent = true;
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;
            _isStopFollowingPath = false;
            _endNodeCache = endNode;

            if (!isFoundPath)
            {
                _isReachedDestination = true;
                return;
            }
            _pathToTarget = newPath;

            _followPathCoroutine = ResetartCoroutine(_followPathCoroutine);

            Coroutine ResetartCoroutine(Coroutine coroutine)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
                return gameObject.activeSelf ? StartCoroutine(FollowPath()) : null;
            }
        }

        IEnumerator FollowPath()
        {
            int startIndex = GetClosestPathPointIndex();
            _isReachedDestination = false;

            _currentWaypoint = _pathToTarget[startIndex];
            _pathIndex = 0;
            while(true)
            {
                if (_isSleeping)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                if(_isStopFollowingPath)
                {
                    StopFollowingPath();
                    _startNodeCache = null;
                    yield break;
                }

                if (IsReachedWayPoint())
                {
                    _pathIndex++;
                    if (_pathIndex >= _pathToTarget.Length)
                    {
                        StopFollowingPath();
                        yield break;
                    }
                    _currentWaypoint = _pathToTarget[_pathIndex];
                }

                _isMoving = true;
                transform.position = Vector2.MoveTowards(transform.position, (Vector2)_currentWaypoint, _appliedSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            void StopFollowingPath()
            {
                _isReachedDestination = true;
                _isMoving = false;
                _currentWaypoint = null;
            }

            int GetClosestPathPointIndex()
            {
                float minDistance = Vector2.Distance(transform.position, _pathToTarget[0]);
                int minIndex = 0;
                for (int i=1; i < _pathToTarget.Length; i++)
                {
                    float distance = Vector2.Distance(transform.position, _pathToTarget[i]); 
                    if (distance <= minDistance)
                    {
                        minDistance = distance;
                        minIndex = i; 
                    }
                    else
                    {
                        if (minIndex == 0)
                            minIndex = 1;
                        break;
                    }
                }
                return minIndex;
            }

            bool IsReachedWayPoint()
            {
                if (!Mathf.Approximately(transform.position.x, ((Vector2)_currentWaypoint).x))
                    return false;
                if (!Mathf.Approximately(transform.position.y, ((Vector2)_currentWaypoint).y))
                    return false;
                return true;
            }
        }

        void Sprint()
        {
            if (IsCanSprint())
                StartSprinting();
            
            if (IsStillSprinting())
                ExhaustSprint();
            else
            {
                if (!_isRecoveringSprint)
                    StartRecoveringSprint();
                
                if (!IsRecoveredSprint())
                    RecoverSprint();
                else
                    StopSprintRecovery();
            }

            bool IsCanSprint()
            {
                return !_isSprinting && !_isRecoveringSprint;
            }
            void StartSprinting()
            {
                _isSprinting = true;
                _appliedSpeed *= _sprintSpeedMultiplier;
                _sprintTimer = _sprintDuration;
            }
            bool IsStillSprinting()
            {
                return _sprintTimer > 0 && _isMoving && !_isRecoveringSprint;
            }
            void ExhaustSprint()
            {
                _sprintTimer -= Time.deltaTime;
            }
            void StartRecoveringSprint()
            {
                _isRecoveringSprint = true;
                _isSprinting = false;
                _appliedSpeed /= _sprintSpeedMultiplier;
            }
            bool IsRecoveredSprint()
            {
                return _sprintTimer > _sprintDuration;
            }
            void RecoverSprint()
            {
                _sprintTimer += Time.deltaTime * _sprintRecoveryMultiplier;
            }
            void StopSprintRecovery()
            {
                if (_sprintTimer > _sprintDuration)
                {
                    _sprintTimer = _sprintDuration;
                    _isRecoveringSprint = false;
                    
                }
                else
                {
                    _isRecoveringSprint = false;
                }
            }
        }
    
        public float GetStaminaPercentage()
        {
            return _sprintTimer / _sprintDuration;
        }

        void UpdateFacingDirection()
        {
            if (_previousPosition == null)
            {
                _previousPosition = transform.position;
                return;
            }

            if (_previousPosition == transform.position)
                return;

            Vector2 newDirection = ((Vector2)transform.position - (Vector2)_previousPosition).normalized;
            _previousPosition = transform.position;
            _facingDirection = newDirection;
        }

        void CheckForCollisions()
        {
            if (_currentWaypoint != null)
            {
                Vector2 difference = (Vector2)_currentWaypoint - (Vector2)transform.position;
                Vector2 direction = difference.normalized;
                float distance = difference.magnitude;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, _collisionLayer);
                
                if (hit.collider != null)
                {
                    _isStopFollowingPath = true;
                    _isCollided = true;
                }
                else
                {
                    _isCollided = false;
                }
            }
        }

        void CheckForSlowdownAreas()
        {
            SlowdownArea  slowdownArea = null;
            Collider2D[] colliders = GetCollisionColliders();
            foreach (Collider2D collider in colliders)
            {
                if (1 << collider.gameObject.layer == _slowdownAreaLayer.value)
                    slowdownArea = collider.gameObject.GetComponent<SlowdownArea>();
            }

            if (slowdownArea == null && _isSlowedDown)
            {
                _appliedSpeed /= slowdownArea.SlowdownFactor;
                _isSlowedDown = false;
            }
            else if (slowdownArea != null && !_isSlowedDown)
            {
                _appliedSpeed *= slowdownArea.SlowdownFactor;
                _isSlowedDown = true;
            }
        }

        Collider2D[] GetCollisionColliders()
        {
            return Physics2D.OverlapBoxAll((Vector2)transform.position + _collisionCheckOffset, _collisionCheckSize, 0f);
        }

        public void Sleep(float time)
        {

            if (_wakeCoroutine == null)
            {
                _isSleeping = true;
                _isMoving = false;
                _currentSleepTime = time;
                _sleepEvent.Invoke();
                _wakeCoroutine = StartCoroutine(Wake());
            }

            IEnumerator Wake()
            {
                yield return new WaitForSeconds(time);
                _isSleeping = false;
                _wakeCoroutine = null;
            }
        }

        public virtual void Die()
        {
            if (_deathEffect != null)
                Instantiate(_deathEffect, transform.position, Quaternion.identity);
            SoundManager.Instance.PlaySoundAtPosition(transform.position, SoundManager.Sound.Death, true);
            gameObject.SetActive(false);
        }
    }
}
