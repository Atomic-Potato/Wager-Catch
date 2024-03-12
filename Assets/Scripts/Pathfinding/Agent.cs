using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pathfinding
{
    [RequireComponent(typeof(Collider2D))]
    public class Agent : MonoBehaviour
    {
        #region Global Variables
        [Min(0)] public float Speed = 10f;
        [SerializeField] protected float _stoppingDistance;
        public float StoppingDisntace => _stoppingDistance;
        [SerializeField] protected LayerMask _agentsLayer;
        public Type SelectedType = Type.A;
        [SerializeField, Min(0)] protected float _neighborsDetectionRadius = 1f;
        public float NeighborsDetectionRadius => _neighborsDetectionRadius;
        
        [Space, Header("Smooth Path")]
        [SerializeField] bool _isUseSmoothPath;
        public bool IsUseSmoothPath {get {return _isUseSmoothPath;} protected set {_isUseSmoothPath = value;}}
        [SerializeField, Min(0)] float _smoothPathTurningDistance;
        public float SmoothPathTurningDistance {get {return _smoothPathTurningDistance;} protected set {_smoothPathTurningDistance = value;}}
        [SerializeField, Min (0)] float _smoothPathTurningSpeed; 
        public float SmoothPathTurningSpeed {get {return _smoothPathTurningSpeed;} protected set {_smoothPathTurningSpeed = value;}} 

        [Space, Header("Other")]
        [SerializeField] protected bool _isRotateWithMovement;


        [Space, Header("Gizmos")]
        [SerializeField] bool _isDrawGizmos;
        [SerializeField] bool _isDrawPath;
        [SerializeField] Color _pathColor = new Color(0f, 0f, 1f, .5f);
        [SerializeField] bool _isRandomPathColor = false;
        [SerializeField] bool _isDrawNeighborsDetectionRadius;

        [HideInInspector] public Transform Target;

        protected Collider2D _collider;

        protected AgentsManager _agentsManager;
        protected AgentBehavior _behavior;
        protected Grid _grid;
        public Grid Grid => _grid;

        Vector2 _currentWaypoint;
        public StraightPath StraightPath {get; protected set;}
        public int PathIndex {get; protected set;}
        public Path SmoothPath {get; protected set;}
        protected Coroutine _followPathCoroutine;
        protected Node _endNodeCache = null;
        
        protected int _priority;
        public int Priority => _priority;
        protected bool _isMoving;
        public bool IsMoving => _isMoving;
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
                if (IsUseSmoothPath && SmoothPath != null)
                {
                    SmoothPath.DrawPathWithGizmos(PathIndex);
                    Gizmos.DrawLine(_currentWaypoint, transform.position);
                }
                else if (StraightPath.Path != null)
                {
                    Gizmos.color = _pathColor;
                    for (int i = PathIndex; i < StraightPath.Path.Length; i++)
                    {
                        if (i > PathIndex)
                            Gizmos.DrawLine(StraightPath.Path[i-1], StraightPath.Path[i]);
                        Gizmos.DrawLine(transform.position, _currentWaypoint); 
                        Gizmos.DrawCube(StraightPath.Path[i], new Vector3(.25f, .25f, 0f));
                    }
                    if (StraightPath.Path.Length > 0)
                        Gizmos.DrawLine(StraightPath.Path[StraightPath.Path.Length - 1], Target.position);
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
            SendPathRequest();
            Move();
        }
        #endregion

        #region Getting a Path
        Coroutine _pathRequestCoroutine;
        void SendPathRequest()
        {
            if (_isPathRequestSent)
                return;
                
            if (_pathRequestCoroutine == null)
                _pathRequestCoroutine = StartCoroutine(SendRequest());

            IEnumerator SendRequest()
            {
                if (Target == null)
                    yield break;

                // Delaying the path request at the start of the game
                // since delta time is quite high at the start
                if (Time.timeSinceLevelLoad < .3f)
                    yield return new WaitForSecondsRealtime(.3f);

                PathRequestManager.RequestPath(new PathRequest(transform.position, Target.position, _grid, _endNodeCache, UpdatePath));
                _isPathRequestSent = true;

                _pathRequestCoroutine = null;
            }
        }

        void UpdatePath(Vector2[] newPath, bool isFoundPath, Node endNode)
        {
            _isPathRequestSent = false;   
            _endNodeCache = endNode;

            if (!isFoundPath)
                return;

            CreatePath();
            UpdateCurrentPathWaypoint();

            void CreatePath()
            {
                if (IsUseSmoothPath)
                    SmoothPath = new Path(newPath, transform.position, _smoothPathTurningDistance, _stoppingDistance);
                else
                {
                    Debug.Log("Created straight path");
                    StraightPath = new StraightPath(newPath, _stoppingDistance);
                }
            }
        }
        #endregion

        #region Moving
        /// <summary>
        /// Calculates the average heading direction from each behavior (such as Follow Path or Avoidance behaviors)
        /// and moves the agent in that direction.
        /// </summary>
        void Move()
        {
            Vector3 velocity = (Vector3)_behavior.CalculateBehaviorVelocity(this, GetNeighbors(), _currentWaypoint);
            transform.position += velocity * Time.deltaTime;
            if (_isRotateWithMovement)
            {
                float angle = Mathf.Atan2(velocity.normalized.y, velocity.normalized.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
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
        }
        
        /// <summary>
        /// Updates the current path waypoint of the agent as the agent moves along the path
        /// </summary>
        void UpdateCurrentPathWaypoint()
        {
            _isReachedDestination = false;
            _isMoving = true;
            PathIndex = 0;
            
            if (_followPathCoroutine != null)
                StopCoroutine(_followPathCoroutine);
            _followPathCoroutine = IsUseSmoothPath ?  StartCoroutine(UpdateSmoothPathWaypoint()) : StartCoroutine(UpdateStraightPathWaypoint());
            
            IEnumerator UpdateStraightPathWaypoint()
            {
                if (StraightPath.Path.Length == 0)
                    yield break;
                _currentWaypoint = StraightPath.Path[0];
                while(true)
                {
                    if (IsReachedCurrentWayPoint())
                    {
                        PathIndex++;
                        if (IsReachedEndOfPath())
                        {
                            FinishPath();
                            yield break;
                        }
                        _currentWaypoint = StraightPath.Path[PathIndex];
                    }
                    yield return new WaitForEndOfFrame();
                }


                bool IsReachedCurrentWayPoint()
                {
                    return Vector2.Distance(transform.position, _currentWaypoint) < 0.01f;
                }
                bool IsReachedEndOfPath()
                {
                    return PathIndex >= StraightPath.Path.Length;
                }
                
            }

            IEnumerator UpdateSmoothPathWaypoint()
            {
                bool isFollowingPath = true;
                _currentWaypoint = SmoothPath.WayPoints[0];
                
                while (isFollowingPath)
                {
                    while (SmoothPath.TurningBoundaries[PathIndex].IsCorssedLine(transform.position))
                    {
                        if (PathIndex == SmoothPath.LastBoundaryIndex)
                        {
                            isFollowingPath = false;
                            FinishPath();
                            break;
                        }
                        else
                        {
                            PathIndex++;
                        }
                    }

                    _currentWaypoint = SmoothPath.WayPoints[PathIndex];
                    yield return null;
                }
            }

            void FinishPath()
            {
                _followPathCoroutine = null;
                _isReachedDestination = true;
                _isMoving = false;
            }
        }

        #endregion
    }

    public struct StraightPath
    {
        public Vector2[] Path;
        public int Length {get; private set;}
        public int LastPointIndex {get; private set;}
        public int StoppingIndex {get; private set;}

        public StraightPath(Vector2[] path, float stoppingDistance)
        {
            Path = path;
            Length = path.Length;
            LastPointIndex = path.Length - 1;
            StoppingIndex = GetStoppingIndex();

            int GetStoppingIndex()
            {
                float distanceFromEndPoint = 0;
                for (int i=path.Length - 1; i > 0; i--)
                {
                    distanceFromEndPoint += Vector2.Distance(path[i], path[i-1]);
                    if (distanceFromEndPoint > stoppingDistance)
                        return i;
                }
                return path.Length - 1;
            }
        }
    }
}
