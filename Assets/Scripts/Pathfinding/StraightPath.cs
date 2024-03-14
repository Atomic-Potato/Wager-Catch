using System;
using UnityEngine;

namespace Pathfinding
{
    public class StraightPath : Path
    {
        public StraightPath(Vector2[] wayPoints, float stoppingDistance)
        {
            WayPoints = wayPoints;
            StoppingIndex = GetStoppingIndex(wayPoints, stoppingDistance);
        }

        public override void DrawPathWithGizmos(int startingIndex, Color pathColor, Vector3? agentPosition = null, Vector3? targetPosition = null)
        {
            if (WayPoints == null)
                return;

            Gizmos.color = pathColor;
            for (int i = startingIndex; i < WayPoints.Length; i++)
            {
                if (i > CurrentPathIndex)
                    Gizmos.DrawLine(WayPoints[i-1], WayPoints[i]);
                if (agentPosition != null)
                    Gizmos.DrawLine((Vector3)agentPosition, CurrentWaypointPosition); 
                Gizmos.DrawCube(WayPoints[i], new Vector3(.25f, .25f, 0f));
            }

            if (targetPosition != null && WayPoints.Length > 0)
                Gizmos.DrawLine(WayPoints[WayPoints.Length - 1], (Vector2)targetPosition);
        }

        public override bool IncrementPathIndex()
        {
            if (IsReachedEndOfPath)
                return false;
            CurrentPathIndex++;
            if (CurrentPathIndex >= WayPoints.Length)
            {
                IsReachedEndOfPath = true;
                CurrentPathIndex--;
            }
            return false;
        }
    }
}
