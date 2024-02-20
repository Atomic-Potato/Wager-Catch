using System.Collections.Generic;
using UnityEngine;
namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Follow Path Behavior", menuName = "Pathfinding/Behavior/Follow Path")]
    public class FollowPathBehavior : AgentBehavior
    {
        public override Vector2 CalculateNextDirection(Agent agent, List<Transform> neighbors, Vector2 destination)
        {
            return destination - (Vector2)agent.transform.position;
        }
    }
}
