using System.Collections.Generic;
using UnityEngine;
namespace Pathfinding
{
    /// <summary>
    /// Calculates the velocity to follow a path, either smooth or straigth
    /// </summary>
    [CreateAssetMenu (fileName = "Follow Path Behavior", menuName = "Pathfinding/Behavior/Follow Path")]
    public class FollowPathBehavior : AgentBehavior
    {
        /// <summary>
        /// A cache to be used for smooth vector rotation in smooth paths
        /// </summary>
        Vector2 moveDirection = Vector2.zero;
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (agent.Path == null || agent.Path.IsReachedEndOfPath)
                return Vector2.zero;

            float speedPercent = GetSlowDownSpeedPercent();     // Used to slow down the agent as it gets closer to the target
            Vector2 velocity = GetVelocity();
            speedPercent = speedPercent > .01f ? speedPercent : speedPercent;
            return velocity * agent.SpeedMultiplier * speedPercent;

            float GetSlowDownSpeedPercent()
            {
                    if (agent.Path.CurrentPathIndex >= agent.Path.StoppingIndex && agent.StoppingDistance > 0)
                    {
                        Path path = agent.Path;
                        float remainingDistance = agent.IsUseSmoothPath ? 
                            ((SmoothPath)path).TurningBoundaries[((SmoothPath)path).LastTruningBoundaryIndex].GetDistanceFromPoint(agent.transform.position):
                            Vector2.Distance(agent.transform.position, path.WayPoints[path.WayPoints.Length - 1]);
                        return Mathf.Clamp01(remainingDistance / agent.StoppingDistance);
                    }
                return 1;
            }
            Vector2 GetVelocity()
            {
                if (!agent.IsUseSmoothPath)
                {
                    return (destination - (Vector2)agent.transform.position).normalized;
                }
                else
                {
                    Vector3 targetDirection = (destination - (Vector2)agent.transform.position).normalized;
                    moveDirection = Vector2.Lerp(moveDirection, targetDirection, Time.deltaTime * agent.SmoothPathTurningSpeed).normalized;
                    return moveDirection;
                }
            }
        }
    }
}
