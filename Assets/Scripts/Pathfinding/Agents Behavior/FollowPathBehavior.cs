using System.Collections.Generic;
using UnityEngine;
namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Follow Path Behavior", menuName = "Pathfinding/Behavior/Follow Path")]
    public class FollowPathBehavior : AgentBehavior
    {
        Vector2 moveDirection = Vector2.zero;
        public override Vector2 CalculateNextDirection(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (agent.IsReachedDestination)
                return Vector2.zero;
                
            if (!agent.IsUseSmoothPath)
            {
                return destination - (Vector2)agent.transform.position;
            }
            else
            {
                if (agent.SmoothPath == null)
                    return Vector2.zero;

                Vector3 targetDirection = (destination - (Vector2)agent.transform.position).normalized;
                moveDirection = Vector2.Lerp(moveDirection, targetDirection, Time.deltaTime * agent.SmoothPathTurningSpeed).normalized;
                return moveDirection;
            }
        }
    }
}
