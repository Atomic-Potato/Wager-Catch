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
    [RequireComponent(typeof(Grid))]
    public class Pathfinder : MonoBehaviour
    {
        [SerializeField] Grid grid;
        [SerializeField] PathRequestManager pathRequestManager;
        
        [Space, Header("Performance")]
        [SerializeField] bool isLogTimeToGetPath;

        Heap<Node> _openSet;
        HashSet<Node> _closedSet;

        void Awake()
        {
            _openSet = new Heap<Node>(grid.MaxSize);
            _closedSet = new HashSet<Node>();
        }

        public void StartFindingPath(PathRequest pathRequest)
        {
            StartCoroutine(FindPathOnGrid(pathRequest.StartPosition, pathRequest.EndPosition, pathRequest.EndNodeCache, pathRequest.StartNodeCache));
        }

        IEnumerator FindPathOnGrid(Vector2 startPosition, Vector2 endPosition, Node endNodeCache = null, Node startNodeCache = null)
        {
            // Debugging
            Stopwatch sw = null;
            if (isLogTimeToGetPath)
            {
                sw = new Stopwatch();
                sw.Start();
            } 

            Node startNode = grid.GetNodeFromWorldPosition(startPosition);
            Node endNode = grid.GetNodeFromWorldPosition(endPosition);

            Vector2[] pathWaypoints = null;
            bool isFoundPath = false;

            if (!IsNodesAreCached())
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
            
            yield return new WaitForEndOfFrame();
            
            if (isFoundPath)
                pathWaypoints = RetracePath(startNode, endNode);

            pathRequestManager.FinishProcessingPathRequest(pathWaypoints, isFoundPath, endNode);
            
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
            bool IsNodesAreCached()
            {
                // return endNode == endNodeCache && startNode == startNodeCache;
                return endNode == endNodeCache;
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
            List<Vector2> waypoints = NodesToPositions(path);
            return waypoints.ToArray();


            List<Vector2> NodesToPositions(List<Node> nodes)
            {
                List<Vector2> positions = new List<Vector2>();
                foreach(Node n in nodes)
                    positions.Add(n.WorldPosition);
                return positions;
            }
        }

        // Removes uneeded nodes from the path by only keeping nodes where the direction changes 
        public static Vector2[] SimplifyPath(Vector2[] path)
        {
            Vector2 previousDirection = Vector2.zero;
            List<Vector2> simplifiedPath = new List<Vector2>();

            for (int i=1; i < path.Length; i++)
            {
                Vector2 newDirection = new Vector2(path[i].x - path[i-1].x, path[i].y - path[i-1].y);
                if (previousDirection != newDirection)
                    simplifiedPath.Add(path[i]);
                previousDirection = newDirection;

                // // The path will sometimes skip a waypoint needed to get around corners
                // // so we check on the last node if the direction with the startNode doesnt match the previous
                // // if so, we add the last node
                // // https://i.imgur.com/KkA3Y3Q.png
                // if (i == path.Length-1)
                // {
                //     Vector2 directionToStartNode = new Vector2(path[i].GridPositionX - startNode.GridPositionX, path[i].GridPositionY - startNode.GridPositionY);
                //     if (directionToStartNode != previousDirection)
                //         simplifiedPath.Add(path[i].WorldPosition);
                // }
            }
                        
            return simplifiedPath.ToArray();
        }
    }
}
