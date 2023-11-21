using System.Collections;
using UnityEngine;

namespace Pathfinding
{
    public class TestUnit : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        [SerializeField] float destinationReachedWaitTime = 2f;
        [SerializeField] Transform target;
        [SerializeField] LayerMask collisionMask;

        [Space, Header("Gizmos")]
        [SerializeField] bool isDrawPath;
        [SerializeField] Color pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool isRandomPathColor = true;

        [HideInInspector] public PathRequestManager PathRequestManager;
        [HideInInspector] public TestUnitsManager TestUnitsManager;

        Vector2 _facingDirection  = Vector2.down; 
        public Vector2 FacingDirection => _facingDirection;
        Vector2? _previousPosition;
        bool _isMoving;
        public bool IsMoving => _isMoving;

        Vector2[] _pathToTarget;
        Coroutine _followPathCoroutine;
        int _pathIndex;
        Vector2 _previousPathStartPoint = Vector2.zero;
        Vector2? _currentWaypoint = null;
        Node _endNodeCache = null;
        Node _startNodeCache = null;
        Vector2 _randomEndNodePositionCache;
        bool _isPathRequestSent;
        bool _isReachedDestination;
        bool _isStopFollowingPath;
        float _waitTimer;

        void OnDrawGizmos()
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

        void Awake()
        {
            if (isRandomPathColor)
                pathColor = new Color(Random.Range(.8f,1f), Random.Range(.4f,8f), Random.Range(0f,4f), 1f);
        }

        void Start()
        {
            SendPathRequest();
        }

        void Update()
        {
            if (_isReachedDestination && !_isPathRequestSent)
            {
                if (_waitTimer < destinationReachedWaitTime)
                    _waitTimer += Time.deltaTime;
                else
                    SendPathRequest();
            }

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

        void SendPathRequest()
        {
            Vector2? targetPosition = target == null ? 
                (_isStopFollowingPath ? _randomEndNodePositionCache : TestUnitsManager.GetRandomWalkableNode()?.WorldPosition) 
                : target.position;
            if (targetPosition == null)
                return;
            PathRequestManager.RequestPath(transform.position, (Vector2)targetPosition, _endNodeCache, _startNodeCache, UpdatePath);
            _isPathRequestSent = true;
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;
            _isStopFollowingPath = false;
            _isReachedDestination = false;
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
            int startIndex;
            _isMoving = true;

            if (_pathToTarget[0] == _previousPathStartPoint)
                startIndex = 1;
            else
            {
                startIndex = 0;
                _previousPathStartPoint = _pathToTarget[0];
            }

            _currentWaypoint = _pathToTarget[startIndex];
            _pathIndex = 0;
            while(true)
            {
                if(_isStopFollowingPath)
                {
                    StopFollowingPath();
                    _startNodeCache = null;
                    _randomEndNodePositionCache = (Vector2)_currentWaypoint;
                    yield break;
                }

                if ((Vector2)transform.position == (Vector2)_currentWaypoint)
                {
                    _pathIndex++;
                    if (_pathIndex >= _pathToTarget.Length)
                    {
                        StopFollowingPath();
                        _waitTimer = 0f;
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
    }
}
