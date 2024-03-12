using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public abstract class AgentBehavior : ScriptableObject
    {
        public abstract Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination);
    }
}
