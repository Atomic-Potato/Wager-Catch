using System.Collections;
using UnityEngine;

namespace Pathfinding
{
    // I shouldve made this at the start of the project but eh thats what u get for not planning
    public class UnitBase : MonoBehaviour
    {
        [SerializeField] protected float _speed = 2f;
        
        [Space, Header("Death")]
        [SerializeField, Min(0f)] float _impulseForce = 1f;
        [SerializeField, Min(.1f)] float _impulsedSpeed = 1f;
        [SerializeField, Min(0f)] float _deathExplosionRadius = 3.5f;
        
        [Tooltip("Lower values imply faster initial force drop off")]
        [SerializeField, Min(.01f)] float _impulseInitialForceDropOff = .2f; 
        [SerializeField] LayerMask _affectedObjectsLayers;
        [SerializeField] Collider2D _impulseCollider;
        [SerializeField] protected Rigidbody2D _rigidbody;

        [Space]
        [SerializeField] Vector2 _collisionCheckSize = Vector2.one;
        [SerializeField] Vector2 _collisionCheckOffset;
        
        [Space]
        [SerializeField] LayerMask _collisionLayer;

        [Space, Header("Gizmos")]
        [SerializeField] bool _isDrawCollisionCheckSize;
        [SerializeField] Color _collisionCheckCollor = Color.red;
        [SerializeField] bool _isDrawPath;
        [SerializeField] Color pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool isRandomPathColor = true;
        [SerializeField] bool _isDrawDeathExplosionRadius;

        [HideInInspector] public Grid grid;
        [HideInInspector] public PathRequestManager UnitPathRequestManager;
        [HideInInspector] public TeamsManager TeamsManager;

        protected Vector2? _target;
        Vector2 _facingDirection  = Vector2.down; 
        public Vector2 FacingDirection => _facingDirection;
        Vector2? _previousPosition;
        protected bool _isMoving;
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
        protected bool _isSleeping;

        float _appliedSpeed;

        Coroutine _impulseCoroutine;

        protected void OnDrawGizmos()
        {
            if(_isDrawPath)
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

            if (_isDrawDeathExplosionRadius)
            {
                Gizmos.DrawWireSphere(transform.position, _deathExplosionRadius);
            }
        }

        protected void Awake()
        {
            if (isRandomPathColor)
                pathColor = new Color(Random.Range(.8f,1f), Random.Range(.4f,8f), Random.Range(0f,4f), 1f);

            _appliedSpeed = _speed;
        }

        protected void Start()
        {
            UnitPathRequestManager = PathRequestManager.Instance;
        }

        protected void Update()
        {
            UpdateFacingDirection();
            CheckForCollisions();
        }

        
        protected void SendPathRequest()
        {
            Vector2? targetPosition = (Vector2)_target;
            if (targetPosition == null || _isSleeping)
                return;
            
            UnitPathRequestManager.RequestPath(transform.position, (Vector2)targetPosition, _endNodeCache, _startNodeCache, UpdatePath);
            _isPathRequestSent = true;
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;
            _isStopFollowingPath = false;
            _endNodeCache = endNode;

            if (!isFoundPath)
                return;
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

                if ((Vector2)transform.position == (Vector2)_currentWaypoint)
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

        Collider2D[] GetCollisionColliders()
        {
            return Physics2D.OverlapBoxAll((Vector2)transform.position + _collisionCheckOffset, _collisionCheckSize, 0f);
        }

        public void Sleep()
        {
            _isSleeping = true;
            _isMoving = false;
        }

        public void Wake()
        {
            _isSleeping = false;
        }

        public void ImpulseFromPoint(Vector2 point)
        {
            if (_impulseCoroutine == null)
                _impulseCoroutine = StartCoroutine(Impulse());
            else
            {
                StopCoroutine(_impulseCoroutine);
                _impulseCoroutine = StartCoroutine(Impulse());
            }
                

            IEnumerator Impulse()
            {
                Sleep();
                _impulseCollider.enabled = true;
                Vector2 direction = ((Vector2)transform.position - point).normalized;
                float x = 0;
                
                do 
                {
                    float dropOff = -_impulseInitialForceDropOff*Mathf.Log10(x);
                    x += Time.deltaTime * _impulsedSpeed;
                    _rigidbody.velocity = direction * _impulseForce * dropOff;
                    yield return null;
                } while (x <= 1f);

                _impulseCollider.enabled = false;
                _rigidbody.velocity = Vector2.zero;
                Wake();
            }
        }

        public virtual void Die()
        {
            ImpulseObjectsInPorximity();
            gameObject.SetActive(false);

            void ImpulseObjectsInPorximity()
            {
                RaycastHit2D[] hits =  Physics2D.CircleCastAll(transform.position, _deathExplosionRadius, Vector2.zero, 50f,  _affectedObjectsLayers);
                foreach (RaycastHit2D hit in hits)
                {
                    TeamPlayer teamPlayer = hit.collider.gameObject.GetComponent<TeamPlayer>();
                    if (teamPlayer != null)
                    {
                        if (teamPlayer is Runner && ((Runner)teamPlayer).IsInSafeArea)
                            continue;

                        teamPlayer.ImpulseFromPoint(transform.position);
                    }
                    else
                    {
                        UnitBase unit = hit.collider.gameObject.GetComponent<UnitBase>();
                        if (unit == this)
                            return;

                        unit.ImpulseFromPoint(transform.position);
                    }
                }
            }
        }
    }
}
