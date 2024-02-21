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
        public override Vector2 CalculateNextDirection(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            bool isNeighborsExist = neighbors.Count != 0;
            if (!isNeighborsExist)
                return Vector2.zero;
                
            return GetNeighborsAvgAvoidancePosition();

            Vector3 GetNeighborsAvgAvoidancePosition()
            {
                Vector3 sum = Vector3.zero;
                int neighborsToAvoidCount = 0;
                foreach (Agent neighbor in neighbors)
                {
                    if (neighbor.Priority < agent.Priority)
                        continue;
                    bool isNeighborWithinAvoidanceDistance =
                        Vector3.Distance(neighbor.transform.position, agent.transform.position) < agent.NeighborsDetectionRadius;
                    if (isNeighborWithinAvoidanceDistance)
                    {
                        sum += agent.transform.position - neighbor.transform.position;
                        neighborsToAvoidCount++;
                    }
                }
                sum *= agent.Priority * agent.Priority;
                return neighborsToAvoidCount > 0 ? sum / neighborsToAvoidCount : sum;
            }
        }
    }
}
