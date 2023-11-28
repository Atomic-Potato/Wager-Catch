using System.Collections;
using UnityEngine;

namespace Pathfinding
{
    public class Player : MonoBehaviour, IPlayer
    {
        [SerializeField] float speed = 10f;
        [SerializeField] LayerMask collisionMask;

        [Space, Header("Gizmos")]
        [SerializeField] bool isDrawPath;
        [SerializeField] Color pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool isRandomPathColor = true;

        [HideInInspector] public Pathfinding.Grid grid;
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
        bool _isStopFollowingPath;

        protected void OnDrawGizmos()
        {
            if(!isDrawPath)
                return;
            
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

        protected void Awake()
        {
            if (isRandomPathColor)
                pathColor = new Color(Random.Range(.8f,1f), Random.Range(.4f,8f), Random.Range(0f,4f), 1f);
        }

        protected void Update()
        {
            UpdateFacingDirection();
            if (_currentWaypoint != null)
            {
                Vector2 difference = (Vector2)_currentWaypoint - (Vector2)transform.position;
                Vector2 direction = difference.normalized;
                float distance = difference.magnitude;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, collisionMask);
                Debug.DrawRay(transform.position, (Vector2)_currentWaypoint - (Vector2)transform.position, Color.red);
                if (hit.collider != null)
                    _isStopFollowingPath = true;
            }
        }

        protected void SendPathRequest()
        {
            Vector2? targetPosition = (Vector2)_target;
            if (targetPosition == null)
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
                return;
            _pathToTarget = newPath;

            if (_followPathCoroutine != null)
                StopCoroutine(_followPathCoroutine);
            _followPathCoroutine = StartCoroutine(FollowPath());
        }

        IEnumerator FollowPath()
        {
            int startIndex = GetClosestPathPointIndex();
            _isMoving = true;
            _isReachedDestination = false;

            _currentWaypoint = _pathToTarget[startIndex];
            _pathIndex = 0;
            while(true)
            {
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
                transform.position = Vector2.MoveTowards(transform.position, (Vector2)_currentWaypoint, speed * Time.deltaTime);
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

        public virtual void Die()
        {
            gameObject.SetActive(false);
        }
    }
}
