using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// The path containing the waypoints and boundaries of a smooth path.
    /// (Further Info: https://youtu.be/NjQjl-ZBXoY?si=AdBJxxU53rdT40rL)
    /// </summary>
    public class Path
    {
        /// <summary>
        /// The actual waypoints returned from A*
        /// </summary>
        public readonly Vector2[] WayPoints;
        /// <summary>
        /// The boundary points that the agent should cross for a smooth path
        /// </summary>
        public readonly Line[] TurningBoundaries;
        /// <summary>
        /// Helps find the last boundary which will be on the same position of the last waypoint
        /// </summary>
        public readonly int LastBoundaryIndex;
        /// <summary>
        /// The path index where the agent should start slowing down
        /// </summary>
        public readonly int StoppingIndex;

        public Path(Vector2[] wayPoints, Vector2 startingPosition, float turningDistance, float stoppingDistance)
        {
            WayPoints = wayPoints;
            TurningBoundaries = new Line[wayPoints.Length];
            LastBoundaryIndex = wayPoints.Length-1;
            ConstructBoundaryPoints();
            StoppingIndex = GetStoppingIndex();
            
            void ConstructBoundaryPoints()
            {
                Vector2 previousPoint = startingPosition;
                for (int i = 0; i < wayPoints.Length; i++)
                {
                    Vector2 directionToPoint = (wayPoints[i] - previousPoint).normalized;
                    // The boundary/turning point is turningDistance away from the actual waypoint  
                    Vector2 turningPoint = (i == LastBoundaryIndex) ? wayPoints[i] : wayPoints[i] - directionToPoint * turningDistance;
                    // We use the previous point as the perpendicular point so we can know what is the approaching side
                    // of the boundary. And in case of turningDistance > distance from current to previous pointing,
                    // we just provide the previous point moved away from the boundary with the same turning distance.
                    TurningBoundaries[i] = new Line(turningPoint, previousPoint - directionToPoint * turningDistance); 

                    previousPoint = wayPoints[i];
                }
            }

            // Gets the index of which node in the path where the agent should start slowing down
            int GetStoppingIndex()
            {
                float distanceFromEndPoint = 0;
                for (int i=wayPoints.Length - 1; i > 0; i--)
                {
                    distanceFromEndPoint += Vector2.Distance(wayPoints[i], wayPoints[i-1]);
                    if (distanceFromEndPoint > stoppingDistance)
                        return i;
                }
                return wayPoints.Length - 1;
            }
        }

        /// <summary>
        /// Draws the path along with the boundary lines
        /// </summary>
        public void DrawPathWithGizmos(int startingIndex)
        {
            for (int i = startingIndex; i <= LastBoundaryIndex; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(WayPoints[i], Vector3.one * .25f);
                TurningBoundaries[i].DrawLineWithGizmos(.5f);
            }
        }
    }
}
