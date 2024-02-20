using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public abstract class AgentBehavior : ScriptableObject
    {
        public abstract Vector2 CalculateNextDirection(Agent agent, List<Transform> neighbors, Vector2 destination);
    }
}
