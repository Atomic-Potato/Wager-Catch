using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class CompositeBehavior : AgentBehavior
    {
        [SerializeField] AgentBehavior[] _behaviors;
        [SerializeField, Min(0)] float[] _weights;

        public override Vector2 CalculateNextPosition(Agent agent, List<Transform> neighbors, Vector2 destination)
        {
            if (_behaviors.Length != _weights.Length)
                throw new System.Exception("Inequal weights count to behaviors!");
            
            Vector3 moveLocation = Vector3.zero;
            for (int i=0; i < _behaviors.Length; i++)
            {
                Vector3 behaviorLocation = _behaviors[i].CalculateNextPosition(agent, neighbors, destination);
                if (behaviorLocation != Vector3.zero)
                {
                    if (behaviorLocation.sqrMagnitude > Mathf.Pow(_weights[i], 2))
                    {
                        behaviorLocation.Normalize();
                        behaviorLocation *= _weights[i];
                    }
                }
                moveLocation += behaviorLocation;
            }
            return moveLocation;
        }
    }
}
