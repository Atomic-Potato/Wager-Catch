using UnityEngine;

namespace Pathfinding
{
    public class Grid : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] bool _isActive;
        [Tooltip("The size of the gird. Its recommended if its a square area.")]
        [SerializeField, Min(0f)] Vector2 worldSize = new Vector2(1f,1f);
        [Tooltip("The area each node covers")]
        [SerializeField, Min(0f)] float nodeRadius = 1f;  
        [SerializeField] LayerMask unwalkableMask;

        [Space, Header("Gizmos")]
        [SerializeField] bool isDisplayGrid;
        [SerializeField] Color gridColor = new Color(1f,1f,1f,.5f);
        [SerializeField] Color walkableNodesColor = new Color(0f, 1f, 0f, .5f);
        [SerializeField] Color unwalkableNodesColor = new Color(1f, 0f, 0f, .5f);
        #endregion

        #region Global Variables
        public int MaxSize => _nodesCountX * _nodesCountY;
        public int NodesCountX => _nodesCountX;
        public int NodesCountY => _nodesCountY;
        
        Node[,] _nodesGrid;
        public Node[,] Nodes => _nodesGrid;
        float _nodeDiameter;
        int _nodesCountX;
        int _nodesCountY; 
        
        #endregion

        void OnDrawGizmos()
        {
            if (!isDisplayGrid)
                return;

            Gizmos.color = gridColor;
            Gizmos.DrawCube(transform.position, worldSize);

            if (_nodesGrid != null)
            {
                foreach (Node n in _nodesGrid)
                {
                    Gizmos.color = n.IsWalkable ? walkableNodesColor : unwalkableNodesColor;
                    Gizmos.DrawCube(n.WorldPosition, new Vector3(_nodeDiameter - .1f, _nodeDiameter - .1f));
                }
            }
        }

        void Awake()
        {
            if (!_isActive)
                return;

            _nodeDiameter = nodeRadius * 2f;
            _nodesCountX = Mathf.RoundToInt(worldSize.x/_nodeDiameter);
            _nodesCountY = Mathf.RoundToInt(worldSize.y/_nodeDiameter);
            // Ajusting world size to fit the grid
            worldSize.x = _nodeDiameter * _nodesCountX;
            worldSize.y = _nodeDiameter * _nodesCountY;
            CreateGrid();
        }

        void CreateGrid()
        {
            _nodesGrid = new Node[_nodesCountX, _nodesCountY];
            GenerateNodes();
            FindNodesNeighbors();

            void GenerateNodes()
            {
                Vector2 worldBottomLeftPosition = (Vector2)transform.position - new Vector2(worldSize.x * .5f, worldSize.y * .5f);
                for (int x = 0; x < _nodesCountX; x++)
                {
                    for (int y = 0; y < _nodesCountY; y++)
                    {
                        Vector2 worldPosition = worldBottomLeftPosition + new Vector2(x * _nodeDiameter + nodeRadius, y * _nodeDiameter + nodeRadius); 
                        bool isWalkable = !Physics2D.OverlapBox(worldPosition, new Vector2(_nodeDiameter, _nodeDiameter), 0f, unwalkableMask);
                        _nodesGrid[x,y] = new Node(isWalkable, worldPosition, x, y);
                    }
                }
            }

            void FindNodesNeighbors()
            {
                foreach (Node n in _nodesGrid)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if(x == 0 && y == 0)
                                continue;

                            int neighboorX = n.GridPositionX + x;
                            int neighboorY = n.GridPositionY + y;
                            if(neighboorX < 0)
                                break;

                            if (neighboorX >= 0 && neighboorX < _nodesCountX && neighboorY >= 0 && neighboorY < _nodesCountY)
                            {
                                n.Neighboors.Add(_nodesGrid[neighboorX,neighboorY]);
                            }
                        }
                    }
                }
            }
        }

        public Node GetNodeFromWorldPosition(Vector2 worldPosition)
        {
            // if we consider the size of the grid in percentage,
            // i.e bottom left would be (0%, 0%) and top right (100%, 100%)
            // then we can mapp it to the array row and col coordinates 
            
            // this is the expanded formula, just for explaining purposes
            // float positionPerecentX = (worldPosition.x + (transform.position.x + worldSize.x) * .5f) / (transform.position.x + worldSize.x);
            // the following way is simplified for performance
            float positionPerecentX = worldPosition.x / (transform.position.x + worldSize.x) + .5f;
            float positionPerecentY = worldPosition.y / (transform.position.y + worldSize.y) + .5f;

            // Note this does not make sense on its own, to simplify,
            // it is just this
            // Mathf.RoundToInt((_nodesCountX - 1) * positionPerecentX)
            // Mathf.RoundToInt((_nodesCountY - 1) * positionPerecentY)
            // but if you round down a percentage multiplied by a grid-1 
            // you can end with a target one node away from your actual current node.
            int x = Mathf.Abs(Mathf.FloorToInt(Mathf.Clamp(_nodesCountX * positionPerecentX, 0, _nodesCountX-1)));
            int y = Mathf.Abs(Mathf.FloorToInt(Mathf.Clamp(_nodesCountY * positionPerecentY, 0, _nodesCountY-1)));

            return _nodesGrid[x,y];
        }
    }
}