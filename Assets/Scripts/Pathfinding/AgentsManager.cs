﻿using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class AgentsManager : Singleton<AgentsManager>
    {
        [SerializeField] Transform _generalTarget;
        public Transform GeneralTarget => _generalTarget;
        [SerializeField] AgentBehavior _generalBehavior;
        public AgentBehavior AgentBehavior => _generalBehavior;

        List<Agent> _agents = new List<Agent>();
        public List<Agent> Agents => _agents;

        int _currentAgentPriority = 0;

        public int GetUniqueAgentID()
        {
            return _currentAgentPriority++;
        }

        public void SetAllAgentsTarget(Transform target)
        {
            foreach (Agent agent in _agents)
            {
                Debug.Log(agent.Target);
                agent.Target = target;
            }
        }
    }
}
