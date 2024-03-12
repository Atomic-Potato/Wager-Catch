using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Composite Behavior", menuName = "Pathfinding/Behavior/Composite")]
    public class CompositeBehavior : AgentBehavior
    {
        [SerializeField] AgentBehavior[] _behaviors;
        [SerializeField, Min(0)] float[] _weights;
        public override Vector2 CalculateBehaviorVelocity(Agent agent, List<Agent> neighbors, Vector2 destination)
        {
            if (_behaviors.Length != _weights.Length)
                throw new System.Exception("Inequal weights count to behaviors!");
            
            float averageSpeed = 0f;
            Vector2 averageDirection = Vector2.zero;
            int totalNonZeroDirections = 0;
            int totalNonZeroSpeeds = 0;
            for (int i=0; i < _behaviors.Length; i++)
            {
                Vector2 behaviorVelocity = _behaviors[i].CalculateBehaviorVelocity(agent, neighbors, destination);
                if (behaviorVelocity.normalized != Vector2.zero)
                {
                    averageDirection += behaviorVelocity.normalized * _weights[i];
                    totalNonZeroDirections++;
                }
                if (behaviorVelocity.magnitude != 0f)
                {
                    averageSpeed += behaviorVelocity.magnitude;
                    totalNonZeroSpeeds++;
                }
            }
            
            if (totalNonZeroDirections == 0 || totalNonZeroSpeeds == 0)
                return Vector2.zero;
            
            averageDirection /= totalNonZeroDirections;
            averageSpeed /= totalNonZeroSpeeds;
            return averageDirection * averageSpeed;
        }
    }
}
