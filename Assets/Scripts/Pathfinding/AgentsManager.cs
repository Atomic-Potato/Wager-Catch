using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class AgentsManager : Singleton<AgentsManager>
    {
        [SerializeField] Transform _generalTarget;
        public Transform GeneralTarget;

        List<Agent> _agents = new List<Agent>();
        public List<Agent> Agents => _agents;

        int _currentAgentPriority = 0;

        void Start()
        {
            if (_generalTarget != null)
                SetAllAgentsTarget(_generalTarget);
        }

        public int GetUniqueAgentID()
        {
            return _currentAgentPriority++;
        }

        public void SetAllAgentsTarget(Transform target)
        {
            foreach (Agent agent in _agents)
                agent.Target = target;
        }
    }
}
