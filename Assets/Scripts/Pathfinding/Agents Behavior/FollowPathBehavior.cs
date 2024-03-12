using System.Collections.Generic;
using UnityEngine;
namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Follow Path Behavior", menuName = "Pathfinding/Behavior/Follow Path")]
    public class FollowPathBehavior : AgentBehavior
    {
        Vector2 moveDirection = Vector2.zero;
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (agent.IsReachedDestination)
                return Vector2.zero;

            float speedPercent = 1;
            Vector2 velocity;
            if (!agent.IsUseSmoothPath)
            {
                if (agent.StraightPath.Path == null)
                    return Vector2.zero;

                if (agent.PathIndex >= agent.StraightPath.StoppingIndex && agent.StoppingDisntace > 0)
                    speedPercent = Mathf.Clamp01(Vector2.Distance(agent.StraightPath.Path[agent.StraightPath.LastPointIndex], agent.transform.position) / agent.StoppingDisntace);
                velocity = (destination - (Vector2)agent.transform.position).normalized;
            }
            else
            {
                if (agent.SmoothPath == null)
                    return Vector2.zero;

                Vector3 targetDirection = (destination - (Vector2)agent.transform.position).normalized;
                moveDirection = Vector2.Lerp(moveDirection, targetDirection, Time.deltaTime * agent.SmoothPathTurningSpeed).normalized;
                velocity = moveDirection;
                if (agent.PathIndex >= agent.SmoothPath.StoppingIndex && agent.StoppingDisntace > 0) 
                    speedPercent = Mathf.Clamp01(Vector2.Distance(agent.SmoothPath.WayPoints[agent.SmoothPath.LastBoundaryIndex], agent.transform.position) / agent.StoppingDisntace);
            }
            
            speedPercent = speedPercent > .01f ? speedPercent : speedPercent;
            return velocity * agent.Speed * speedPercent;
        }
    }
}
