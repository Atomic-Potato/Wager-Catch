using System.Collections;
using UnityEngine;

namespace Pathfinding
{
    public class TestUnit : MonoBehaviour
    {
        [SerializeField] float speed = 10f;
        [SerializeField] float destinationReachedWaitTime = 2f;
        [SerializeField] Transform target;

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
        Node _endNodeCache = null;
        bool _isPathRequestSent;
        bool _isReachedDestination;
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
        }

        void SendPathRequest()
        {
            Vector2? targetPosition = target == null ? TestUnitsManager.GetRandomWalkableNode()?.WorldPosition : target.position;
            if (targetPosition == null)
                return;
            PathRequestManager.RequestPath(transform.position, (Vector2)targetPosition, _endNodeCache, UpdatePath);
            _isPathRequestSent = true;
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;   
            _endNodeCache = endNode;

            Debug.Log(gameObject.name + " " + isFoundPath);
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
            _isReachedDestination = false;
            _isMoving = true;
            if (_pathToTarget[0] == _previousPathStartPoint)
                startIndex = 1;
            else
            {
                startIndex = 0;
                _previousPathStartPoint = _pathToTarget[0];
            }

            Vector2 currentWaypoint = _pathToTarget[startIndex];
            _pathIndex = 0;
            while(true)
            {
                if ((Vector2)transform.position == currentWaypoint)
                {
                    _pathIndex++;
                    if (_pathIndex >= _pathToTarget.Length)
                    {
                        _isReachedDestination = true;
                        _isMoving = false;
                        _waitTimer = 0f;
                        yield break;
                    }
                    currentWaypoint = _pathToTarget[_pathIndex];
                }
                transform.position = Vector2.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
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
