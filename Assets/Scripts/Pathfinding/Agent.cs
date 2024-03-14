using System;
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

        public Path Path {get; protected set;}
        protected Node _endNodeCache = null;

        public int Priority {get; protected set;}
        public bool IsMoving {get; protected set;}
        protected bool _isPathRequestSent;
        public bool IsPathRequestSent => _isPathRequestSent;

        public enum Type {A, B, C, D, E,}
        #endregion

        #region Execution
        void OnDrawGizmos()
        {
            if (!_isDrawGizmos)
                return;

            if (_isDrawPath && Path != null)
            {
                Path.DrawPathWithGizmos(Path.CurrentPathIndex, _pathColor, transform.position, Target.position);
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
                _pathColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);
        }

        void Start()
        {
            _agentsManager = AgentsManager.Instance;
            _agentsManager.Agents.Add(this);
            if (_agentsManager.GeneralTarget != null)
                Target = _agentsManager.GeneralTarget;
            _behavior = _agentsManager.AgentBehavior;

            Priority = AgentsManager.Instance.GetUniqueAgentID();
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

            Path = CreatePath();
            UpdatePathIndex();

            Path CreatePath()
            {
                return IsUseSmoothPath ? 
                    (Path) new SmoothPath(newPath, transform.position, _smoothPathTurningDistance, _stoppingDistance) :
                    (Path) new StraightPath(newPath, _stoppingDistance);
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
            Vector2 currentWaypoint = Path == null ? Vector2.zero : Path.CurrentWaypointPosition;
            Vector3 velocity = (Vector3)_behavior.CalculateBehaviorVelocity(this, GetNeighbors(), currentWaypoint);
            if (velocity != Vector3.zero)
            {
                IsMoving = true;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                IsMoving = false;
            }

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

        Coroutine _updatePathIndexCoroutine;
        /// <summary>
        /// Updates the current waypoint and path index based on the agent position.
        /// </summary>
        void UpdatePathIndex()
        {
            // NOTE:    I have tried to place the methods inside of the paths classes
            //          but you cant create a new instance of class that uses MonoBehavior
            //          (because you need IEnumerator). 
            //          The other solution was to add a new component instead of using the
            //          new keyword, but that felt messy in my opinion.

            if (_updatePathIndexCoroutine != null)
                StopCoroutine(_updatePathIndexCoroutine);
            _updatePathIndexCoroutine = IsUseSmoothPath ?  
                StartCoroutine(UpdateSmoothPathWaypoint()) : 
                StartCoroutine(UpdateStraightPathWaypoint());
            
            IEnumerator UpdateStraightPathWaypoint()
            {
                StraightPath straightPath = (StraightPath)Path;
                if (straightPath.WayPoints.Length == 0)
                    yield break;

                while(true)
                {
                    bool isReachedCurrentWayPoint = Vector2.Distance(transform.position, straightPath.CurrentWaypointPosition) < 0.01f;
                    if (isReachedCurrentWayPoint)
                    {
                        straightPath.IncrementPathIndex();
                        if (straightPath.IsReachedEndOfPath)
                        {
                            FinishPath();
                            yield break;
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
            }

            IEnumerator UpdateSmoothPathWaypoint()
            {
                SmoothPath smoothPath = (SmoothPath)Path;

                while (true)
                {
                    while (smoothPath.TurningBoundaries[smoothPath.CurrentPathIndex].IsCorssedLine(transform.position))
                    {
                        if (smoothPath.IsReachedEndOfPath)
                        {
                            FinishPath();
                            yield break;
                        }
                        else
                        {
                            smoothPath.IncrementPathIndex();
                        }
                    }
                    yield return null;
                }
            }

            void FinishPath()
            {
                _updatePathIndexCoroutine = null;
            }
        }
        #endregion
    }
}