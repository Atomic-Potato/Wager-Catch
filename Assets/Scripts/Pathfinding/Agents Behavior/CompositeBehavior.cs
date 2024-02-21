using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [CreateAssetMenu (fileName = "Composite Behavior", menuName = "Pathfinding/Behavior/Composite")]
    public class CompositeBehavior : AgentBehavior
    {
        [SerializeField] AgentBehavior[] _behaviors;
        [SerializeField, Min(0)] float[] _weights;
        public static Vector2 Direction;
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vector2.zero, Vector2.right);
        }

        public override Vector2 CalculateNextDirection(Agent agent, List<Transform> neighbors, Vector2 destination)
        {
            if (_behaviors.Length != _weights.Length)
                throw new System.Exception("Inequal weights count to behaviors!");
            
            Vector3 direction = Vector3.zero;
            for (int i=0; i < _behaviors.Length; i++)
            {
                Vector3 behaviorDirection = _behaviors[i].CalculateNextDirection(agent, neighbors, destination);
                if (behaviorDirection != Vector3.zero)
                {
                    // if (behaviorLocation.sqrMagnitude > Mathf.Pow(_weights[i], 2))
                    // {
                    //     behaviorLocation.Normalize();
                    //     behaviorLocation *= _weights[i];
                    // }
                    
                }
                Direction = behaviorDirection * _weights[i];
                Debug.Log(Direction);
                direction += behaviorDirection * _weights[i];
            }
            return (direction/_behaviors.Length).normalized;
        }
    }
}
