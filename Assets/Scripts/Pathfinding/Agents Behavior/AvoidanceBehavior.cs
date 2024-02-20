using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Avoidance Behavior", menuName = "Pathfinding/Behavior/Avoidance")]
    public class AvoidanceBehavior : AgentBehavior
    {
        public override Vector2 CalculateNextPosition(Agent agent, List<Transform> neighbors, Vector2 destination)
        {
            bool isNeighborsExist = neighbors.Count != 0;
            if (!isNeighborsExist)
                return agent.transform.position;
                
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
