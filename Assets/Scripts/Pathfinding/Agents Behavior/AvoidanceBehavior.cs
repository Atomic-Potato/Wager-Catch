using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Avoidance Behavior", menuName = "Pathfinding/Behavior/Avoidance")]
    public class AvoidanceBehavior : AgentBehavior
    {
        [SerializeField] bool _isUseCongestionControl;

        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            bool isNeighborsExist = neighbors.Count != 0;
            if (!isNeighborsExist)
                return Vector2.zero;
                
            return GetNeighborsAvgAvoidancePosition() * agent.Speed;

            Vector3 GetNeighborsAvgAvoidancePosition()
            {
                Vector3 sum = Vector3.zero;
                int neighborsToAvoidCount = 0;
                foreach (Agent neighbor in neighbors)
                {
                    if (_isUseCongestionControl && neighbor.Priority < agent.Priority)
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
