using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class AgentsManager : Singleton<AgentsManager>
    {
        [SerializeField, Min(0)] int _agentsCount;
        [SerializeField] Transform _agentsParent;
        [SerializeField] Grid grid;
        [SerializeField] PathRequestManager pathRequestManager;
        [SerializeField] Transform target;

        List<Agent> _agents = new List<Agent>();
        public List<Agent> Agents => _agents;
        int _currentUnitObjectIndex = 0;

        int _currentAgentPriority = 0;

        public int GetUniqueAgentID()
        {
            return _currentAgentPriority++;
        }
    }
}
