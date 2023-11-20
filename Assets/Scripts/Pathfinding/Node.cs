using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node : IHeapItem<Node>
    {
        public Node Parent;

        public int GridPositionX;
        public int GridPositionY;
        
        public int G_Cost; // distance from starting node
        public int H_Cost; // distance from end node
        public int F_Cost => H_Cost + G_Cost; 

        int _heapIndex;
        public int HeapIndex
        {
            get
            {
                return _heapIndex;
            }
            set
            {
                _heapIndex = value;
            }
        }

        public bool IsWalkable;
        public bool IsSafe;
        public Vector2 WorldPosition;
        public List<Node> Neighboors;

        public Node(bool isWalkable, Vector2 worldPosition, int gridPositionX, int gridPositionY)
        {
            IsWalkable = isWalkable;
            WorldPosition = worldPosition;
            GridPositionX = gridPositionX;
            GridPositionY = gridPositionY;
            Neighboors = new List<Node>();
        }

        /// <summary>
        /// Finds which node has a higher priority
        /// in other words, which is closer to the target
        /// </summary>
        public int CompareTo(Node n)
        {
            // Note: The CompareTo method returns a priority. In case of interger, larger means higher
            //       higher priority = 1
            //       equal priority = 0   
            //       lower priority = -1

            int result = F_Cost.CompareTo(n.F_Cost);
            if (result == 0)
                result = H_Cost.CompareTo(n.H_Cost);

            // Since we want the prirority to be higher for lower costs, we simply reverse our result
            return -result;
        }
    }
}
