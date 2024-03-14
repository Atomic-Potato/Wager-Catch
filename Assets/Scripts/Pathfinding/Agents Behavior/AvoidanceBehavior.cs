using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// A basic behavior to avoid other agent in the path preventing any overlap with other agents.
    /// It is based on BOIDs (flock simulation)
    /// </summary>
    [CreateAssetMenu (fileName = "Avoidance Behavior", menuName = "Pathfinding/Behavior/Avoidance")]
    public class AvoidanceBehavior : AgentBehavior
    {
        [Tooltip(
            "Toggles agents priority. Agents with greater priority than others are not affected by other agents." +
            "This opption helps remove point contesting (When 2 or more agents are heading to the same destination)")]
        [SerializeField] bool _isUseCongestionControl;
        
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            bool isNeighborsExist = neighbors.Count != 0;
            if (!isNeighborsExist)
                return Vector2.zero;
                
            return GetNeighborsAvgAvoidancePosition() * agent.SpeedMultiplier;

            // In BOIDs, this function is the avoidance rule. Taking the sum of the opposite direction from
            // each neighbor, and returning the average.
            Vector3 GetNeighborsAvgAvoidancePosition()
            {
                Vector3 sum = Vector3.zero;
                int neighborsToAvoidCount = 0;
                foreach (Agent neighbor in neighbors)
                {
                    bool isGreaterPriorityThanNeighbor = _isUseCongestionControl && neighbor.Priority < agent.Priority; 
                    if (isGreaterPriorityThanNeighbor)
                        continue;

                    bool isNeighborWithinAvoidanceDistance =
                        Vector3.Distance(neighbor.transform.position, agent.transform.position) < agent.NeighborsDetectionRadius;
                    if (isNeighborWithinAvoidanceDistance)
                    {
                        sum += agent.transform.position - neighbor.transform.position;
                        neighborsToAvoidCount++;
                    }
                }

                // i dont remember why i multiplied the sum with the square of the agent priority
                // cant bother testing why
                sum *= agent.Priority * agent.Priority;
                return neighborsToAvoidCount > 0 ? sum / neighborsToAvoidCount : sum;
            }
        }
    }
}
