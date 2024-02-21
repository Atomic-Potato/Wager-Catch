using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Avoidance Behavior", menuName = "Pathfinding/Behavior/Avoidance")]
    public class AvoidanceBehavior : AgentBehavior
    {

        // TODO:
        // - Make avoidance get strong the closer to the center are the agents
        // - Give random priorities to agents so they would not contest a destination
        public override Vector2 CalculateNextDirection(Agent agent, List<Transform> neighbors, Vector2 destination)
        {
            bool isNeighborsExist = neighbors.Count != 0;
            if (!isNeighborsExist)
                return Vector2.zero;
                
            return GetNeighborsAvgAvoidancePosition();

            Vector3 GetNeighborsAvgAvoidancePosition()
            {
                Vector3 sum = Vector3.zero;
                int neighborsToAvoidCount = 0;
                foreach (Transform neighbor in neighbors)
                {
                    bool isNeighborWithinAvoidanceDistance =
                        Vector3.Distance(neighbor.position, agent.transform.position) < agent.NeighborsDetectionRadius;
                    if (isNeighborWithinAvoidanceDistance)
                    {
                        sum += agent.transform.position - neighbor.position;
                        neighborsToAvoidCount++;
                    }
                }
                return neighborsToAvoidCount > 0 ? sum / neighborsToAvoidCount : sum;
            }
        }
    }
}
