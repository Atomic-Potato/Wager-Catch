using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System;

namespace Pathfinding
{
    // G cost: distance from starting node
    // H cost: distance from end node
    // F cost: H + G
    public class Pathfinder : MonoBehaviour
    {
        [Space, Header("Performance")]
        [SerializeField] bool isLogTimeToGetPath;

        Heap<Node> _openSet;
        HashSet<Node> _closedSet;
        PathRequestManager _pathRequestManager;

        void Start()
        {
            _openSet = new Heap<Node>(GridsManager.GlobalMaxSize);
            _closedSet = new HashSet<Node>();
            _pathRequestManager = PathRequestManager.Instance;
        }


        public void FindPathOnGrid(PathRequest request, Action<PathRequestResult> callback)
        {
            // Debugging
            Stopwatch sw = null;
            if (isLogTimeToGetPath)
            {
                sw = new Stopwatch();
                sw.Start();
            } 

            Node startNode = request.Grid.GetNodeFromWorldPosition(request.StartPosition);
            Node endNode = request.Grid.GetNodeFromWorldPosition(request.EndPosition);

            Vector2[] pathWaypoints = null;
            bool isFoundPath = false;

            if (!IsSameAsPreviousEndNode())
            {
                // This is the equivalent of creating a new list, but without generating garbage
                _openSet.Clear();
                _closedSet.Clear();
                _openSet.Add(startNode);
                    
                while (_openSet.Count > 0)
                {
                    // The first node in the heap is the one with the lowest F cost
                    Node currentNode = _openSet.RemoveFirst();
                    _closedSet.Add(currentNode);

                    if (currentNode == endNode)
                    {
                        if (isLogTimeToGetPath)
                        {
                            sw?.Stop();
                            UnityEngine.Debug.Log("Time: " + sw.ElapsedMilliseconds + "ms");
                        }
                        isFoundPath = true;
                        break;
                    }

                    UpddateNeighboors(currentNode);
                }
            }
            
            if (isFoundPath)
                pathWaypoints = RetracePath(startNode, endNode);

            callback(new PathRequestResult(pathWaypoints, isFoundPath, endNode, request.Callback));
            
            void UpddateNeighboors(Node currentNode)
            {
                foreach (Node neighbor in currentNode.Neighboors)
                {
                    if (!neighbor.IsWalkable || _closedSet.Contains(neighbor))
                        continue;
                    
                    int distanceToNeighboorUsingCurrentPath = currentNode.G_Cost + GetDistanceToNode(currentNode, neighbor);
                    if (distanceToNeighboorUsingCurrentPath < neighbor.G_Cost ||!_openSet.Contains(neighbor))
                    {
                        neighbor.G_Cost = distanceToNeighboorUsingCurrentPath;
                        neighbor.H_Cost = GetDistanceToNode(neighbor, endNode);

                        neighbor.Parent = currentNode;

                        if (!_openSet.Contains(neighbor))
                            _openSet.Add(neighbor);
                        else
                            _openSet.UpdateItem(neighbor);
                    }
                }
            }
            bool IsSameAsPreviousEndNode()
            {
                return endNode == request.EndNodeCache;
            }
        }

        int GetDistanceToNode(Node a, Node b)
        {
            // It is aggreed upon that in A* pathfinding that 
            // diagnoally adjacent nodes have a distance of 14 (touching corners)
            // and parallel adjacent nodes have a distance of 10 (touching borders)
            // So the distnace between nodes is the sum of straight and diagonal moves 
            // taken to reach that node
            int distanceX = Mathf.Abs(a.GridPositionX - b.GridPositionX);
            int distanceY = Mathf.Abs(a.GridPositionY - b.GridPositionY);

            int greaterDistance;
            int smallerDistance;
            if (distanceX > distanceY)
            {
                greaterDistance = distanceX;
                smallerDistance = distanceY;
            }
            else
            {
                greaterDistance = distanceY;
                smallerDistance = distanceX;
            }

            return 14 * smallerDistance + 10 * (greaterDistance - smallerDistance);
        }

        Vector2[] RetracePath(Node startNode, Node endNode)
        {
            if (startNode == null || endNode == null)
                return null;

            Node currentNode = endNode;
            List<Node> path = new List<Node>();
            
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;    
            }

            path.Add(currentNode);  
            path.Reverse();
            return SimplifyPath();

            // Removes uneeded nodes from the path by only keeping nodes where the direction changes 
            Vector2[] SimplifyPath()
            {
                Vector2 previousDirection = Vector2.zero;
                List<Vector2> simplifiedPath = new List<Vector2>();

                for (int i=1; i < path.Count; i++)
                {
                    Vector2 newDirection = new Vector2(path[i].GridPositionX - path[i-1].GridPositionX, path[i].GridPositionY - path[i-1].GridPositionY);
                    if (previousDirection != newDirection)
                        simplifiedPath.Add(path[i].WorldPosition);
                    previousDirection = newDirection;

                    // The path will sometimes skip a waypoint needed to get around corners
                    // so we check on the last node if the direction with the startNode doesnt match the previous
                    // if so, we add the last node
                    // https://i.imgur.com/KkA3Y3Q.png
                    if (i == path.Count-1)
                    {
                        Vector2 directionToStartNode = new Vector2(path[i].GridPositionX - startNode.GridPositionX, path[i].GridPositionY - startNode.GridPositionY);
                        if (directionToStartNode != previousDirection)
                            simplifiedPath.Add(path[i].WorldPosition);
                    }
                }
                            
                return simplifiedPath.ToArray();
            }
        }
    }
}
