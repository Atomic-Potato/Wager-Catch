using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    // TODO:
    // - Optimize the path to have only the corners and the start & end nodes
    [RequireComponent(typeof(Collider2D))]
    public class Agent : MonoBehaviour
    {
        #region Global Variables
        [Min(0)] public float Speed = 10f;
        public Transform Target;
        public PathRequestManager PathRequestManager;
        [SerializeField] LayerMask _agentsLayer;
        [SerializeField] AgentBehavior _agentBehavior;
        [SerializeField, Min(0)] float _neighborsDetectionRadius = 1f;
        public float NeighborsDetectionRadius => _neighborsDetectionRadius;

        [Space, Header("Gizmos")]
        [SerializeField] bool _isDrawGizmos;
        [SerializeField] bool _isDrawPath;
        [SerializeField] Color _pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool _isRandomPathColor = false;
        [SerializeField] bool _isDrawNeighborsDetectionRadius;

        [HideInInspector] public AgentsManager AgentsManager;

        Collider2D _collider;
        Vector2? _previousPosition;

        Vector2[] _pathToTarget;
        Coroutine _followPathCoroutine;
        int _pathIndex;
        Vector2 _previousPathStartPoint = Vector2.zero;
        Node _endNodeCache = null;
        
        int _priority;
        public int Priority => _priority;
        bool _isMoving;
        public bool IsMoving => _isMoving;
        Vector2 _facingDirection  = Vector2.down; 
        public Vector2 FacingDirection => _facingDirection;
        bool _isPathRequestSent;
        public bool IsPathRequestSent => _isPathRequestSent;
        bool _isReachedDestination;
        public bool IsReachedDestination => _isReachedDestination;
        #endregion

        #region Execution
        void OnDrawGizmos()
        {
            if (!_isDrawGizmos)
                return;

            if (_isDrawPath && _pathToTarget != null)
            {
                for(int i = _pathIndex; i < _pathToTarget.Length; i++)
                {
                    Gizmos.color = _pathColor;
                    Gizmos.DrawCube(_pathToTarget[i], new Vector3(.25f, .25f, 0f));
                }
            }

            if (_isDrawNeighborsDetectionRadius)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, _neighborsDetectionRadius);
            }
        }

        void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_isRandomPathColor)
                _pathColor = new Color(Random.Range(.8f,1f), Random.Range(.4f,8f), Random.Range(0f,4f), 1f);
        }

        void Start()
        {
            AgentsManager.Instance.Agents.Add(this);
            _priority = AgentsManager.Instance.GetUniqueAgentID();
            SendPathRequest();
        }

        void Update()
        {
            if (!_isPathRequestSent)
            {
                    SendPathRequest();
            }

            UpdateFacingDirection();

            Debug.DrawLine((Vector2)transform.position, (Vector2)transform.position + CompositeBehavior.Direction, Color.red);
        }
        #endregion

        #region Getting a Path
        void SendPathRequest()
        {
            if (Target == null)
                return;
            PathRequestManager.RequestPath(transform.position, Target.position, _endNodeCache, UpdatePath);
            _isPathRequestSent = true;
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;   
            _endNodeCache = endNode;

            if (!isFoundPath)
                return;
            _pathToTarget = newPath;

            if (_followPathCoroutine != null)
                StopCoroutine(_followPathCoroutine);
            _followPathCoroutine = StartCoroutine(FollowPath());
        }
        #endregion

        IEnumerator FollowPath()
        {
            if (_pathToTarget.Length == 0)
                yield break;
            // int startIndex = GetPathStartIndex();
            int startIndex = 0;
            Vector2 currentWaypoint = _pathToTarget[startIndex];
            _isReachedDestination = false;
            _isMoving = true;
            _pathIndex = 0;

            while(true)
            {
                if (IsReachedCurrentWayPoint())
                {
                    _pathIndex++;
                    if (IsReachedEndOfPath())
                    {
                        FinishPath();
                        yield break;
                    }
                    currentWaypoint = _pathToTarget[_pathIndex];
                }
                MoveToNextWaypoint();
                yield return new WaitForEndOfFrame();
            }

            int GetPathStartIndex()
            {
                // if (_pathToTarget.Length > 1 &&  _pathToTarget[0] == _previousPathStartPoint)
                //     return 1;
                if (_pathToTarget.Length > 1)
                    return 1;
                else
                {
                    _previousPathStartPoint = _pathToTarget[0];
                    return 0;
                }
            }
            bool IsReachedCurrentWayPoint()
            {
                // TODO:
                // Change the logic so that the agent has to either
                // - Reach the waypoint exactly
                // - Stays in the proximity of the waypoint long enough
                // This is to account for when adding local avoidance

                // return (Vector2)transform.position == currentWaypoint;
                return Vector2.Distance(transform.position, currentWaypoint) < 0.01f;
            }
            bool IsReachedEndOfPath()
            {
                return _pathIndex >= _pathToTarget.Length;
            }
            void FinishPath()
            {
                _isReachedDestination = true;
                _isMoving = false;
            }
            List<Agent> GetNeighbors()
            {
                RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _neighborsDetectionRadius, Vector2.zero, Mathf.Infinity, _agentsLayer);
                List<Agent> neighbors = new List<Agent>();
                foreach(RaycastHit2D hit in hits)
                {
                    if (hit.collider != _collider)
                        neighbors.Add(hit.collider.gameObject.GetComponent<Agent>());
                }
                return neighbors;
            }
            void MoveToNextWaypoint()
            {
                Vector3 direction = (Vector3)_agentBehavior.CalculateNextDirection(this, GetNeighbors(), currentWaypoint).normalized;
                transform.position += direction * Speed * Time.deltaTime;
                // transform.position = Vector2.MoveTowards(transform.position, nextDestination, Speed * Time.deltaTime);
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
