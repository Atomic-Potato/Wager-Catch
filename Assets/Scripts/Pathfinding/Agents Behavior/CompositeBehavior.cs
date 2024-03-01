using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Composite Behavior", menuName = "Pathfinding/Behavior/Composite")]
    public class CompositeBehavior : AgentBehavior
    {
        [SerializeField] AgentBehavior[] _behaviors;
        [SerializeField, Min(0)] float[] _weights;
        public override Vector2 CalculateNextDirection(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (_behaviors.Length != _weights.Length)
                throw new System.Exception("Inequal weights count to behaviors!");
            
            Vector3 direction = Vector3.zero;
            for (int i=0; i < _behaviors.Length; i++)
            {
                Vector3 behaviorDirection = _behaviors[i].CalculateNextDirection(agent, neighbors, destination);
                direction += behaviorDirection * _weights[i];
            }
            return direction/_behaviors.Length;
        }
    }
}
