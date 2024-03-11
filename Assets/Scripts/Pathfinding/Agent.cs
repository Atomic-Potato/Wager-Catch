using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [RequireComponent(typeof(Collider2D))]
    public class Agent : MonoBehaviour
    {
        #region Global Variables
        [Min(0)] public float Speed = 10f;
        [SerializeField] protected LayerMask _agentsLayer;
        public Type SelectedType = Type.A;
        [SerializeField, Min(0)] protected float _neighborsDetectionRadius = 1f;
        public float NeighborsDetectionRadius => _neighborsDetectionRadius;
        
        [Space, Header("Path")]
        [SerializeField] protected bool _isUseSmoothPath;
        [SerializeField, Min(0)] protected float _smoothPathTurningDistance;
        [SerializeField, Min (0)] protected float _smoothPathTurningSpeed; 


        [Space, Header("Gizmos")]
        [SerializeField] bool _isDrawGizmos;
        [SerializeField] bool _isDrawPath;
        [SerializeField] Color _pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool _isRandomPathColor = false;
        [SerializeField] bool _isDrawNeighborsDetectionRadius;

        [HideInInspector] public Transform Target;

        protected Collider2D _collider;
        protected Vector2? _previousPosition;

        protected AgentsManager _agentsManager;
        protected AgentBehavior _behavior;
        protected Grid _grid;
        public Grid Grid => _grid;

        Vector2 _currentWaypoint;
        protected Vector2[] _pathToTarget;
        protected int _pathIndex;
        protected Path _smoothPath;
        protected Coroutine _followPathCoroutine;
        protected Node _endNodeCache = null;
        
        protected int _priority;
        public int Priority => _priority;
        protected bool _isMoving;
        public bool IsMoving => _isMoving;
        protected Vector2 _facingDirection  = Vector2.down; 
        public Vector2 FacingDirection => _facingDirection;
        protected bool _isPathRequestSent;
        public bool IsPathRequestSent => _isPathRequestSent;
        protected bool _isReachedDestination;
        public bool IsReachedDestination => _isReachedDestination;

        public enum Type
        {
            A,
            B,
            C,
            D,
            E,
        }
        #endregion

        #region Execution
        void OnDrawGizmos()
        {
            if (!_isDrawGizmos)
                return;

            if (_isDrawPath)
            {
                if (_isUseSmoothPath && _smoothPath != null)
                {
                    _smoothPath.DrawPathWithGizmos();
                }
                else if (_pathToTarget != null)
                {
                    Gizmos.color = _pathColor;
                    for (int i = _pathIndex; i < _pathToTarget.Length; i++)
                    {
                        if (i > _pathIndex)
                            Gizmos.DrawLine(_pathToTarget[i-1], _pathToTarget[i]);
                        Gizmos.DrawLine(transform.position, _currentWaypoint); 
                        Gizmos.DrawCube(_pathToTarget[i], new Vector3(.25f, .25f, 0f));
                    }
                    if (_pathToTarget.Length > 0)
                        Gizmos.DrawLine(_pathToTarget[_pathToTarget.Length - 1], Target.position);
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
                _pathColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        }

        void Start()
        {
            _agentsManager = AgentsManager.Instance;
            _agentsManager.Agents.Add(this);
            if (_agentsManager.GeneralTarget != null)
                Target = _agentsManager.GeneralTarget;
            _behavior = _agentsManager.AgentBehavior;

            _priority = AgentsManager.Instance.GetUniqueAgentID();
            _grid = GridsManager.Instance.GetGrid(SelectedType);
        }

        void Update()
        {
            if (!_isPathRequestSent)
                SendPathRequest();
            Move();
            UpdateFacingDirection();
        }
        #endregion

        #region Getting a Path
        void SendPathRequest()
        {
            if (Target == null)
                return;
            PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, _grid, _endNodeCache, UpdatePath));
            _isPathRequestSent = true;
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;   
            _endNodeCache = endNode;

            if (!isFoundPath)
                return;

            if (_isUseSmoothPath)
                _smoothPath = new Path(newPath, transform.position, _smoothPathTurningDistance);
            else
                _pathToTarget = newPath;

            if (_followPathCoroutine != null)
                StopCoroutine(_followPathCoroutine);
            _followPathCoroutine = _isUseSmoothPath ?  StartCoroutine(FollowSmoothPath()) : StartCoroutine(FollowStraightPath());
        }
        #endregion

        #region Moving
        void Move()
        {
            Vector3 direction = (Vector3)_behavior.CalculateNextDirection(this, GetNeighbors(), _currentWaypoint).normalized;
            transform.position += direction * Speed * Time.deltaTime;

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
        }

        IEnumerator FollowStraightPath()
        {
            if (_pathToTarget.Length == 0)
                yield break;

            int startIndex = 0;
            _currentWaypoint = _pathToTarget[startIndex];
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
                    _currentWaypoint = _pathToTarget[_pathIndex];
                }
                yield return new WaitForEndOfFrame();
            }

            bool IsReachedCurrentWayPoint()
            {
                return Vector2.Distance(transform.position, _currentWaypoint) < 0.01f;
            }
            bool IsReachedEndOfPath()
            {
                return _pathIndex >= _pathToTarget.Length;
            }
            void FinishPath()
            {
                _currentWaypoint = Target.transform.position;
                _isReachedDestination = true;
                _isMoving = false;
            }
        }

        IEnumerator FollowSmoothPath()
        {
            while (true){
                yield return null;
            }
        }

        #endregion

        #region Other
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
        #endregion
    }
}
