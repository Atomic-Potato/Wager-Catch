using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class AgentsManager : MonoBehaviour
    {
        [SerializeField, Min(0)] int _agentsCount;
        [SerializeField] GameObject[] _agents;
        [SerializeField] Transform _agentsParent;
        [SerializeField] Grid grid;
        [SerializeField] PathRequestManager pathRequestManager;
        [SerializeField] Transform target;

        List<Agent> _units = new List<Agent>();
        int _currentUnitObjectIndex = 0;        

        void Start()
        {
            SpawnUnits();
        }

        void SpawnUnits()
        {
            for (int i=0; i < grid.NodesCountX; i++)
            {
                for (int j=0; j < grid.NodesCountY; j++)
                {
                    if (!grid.Nodes[i,j].IsWalkable)
                        continue;
                    GameObject unitObject = GetUnitObject();
                    GameObject spawnedUnit = Instantiate(unitObject, grid.Nodes[i,j].WorldPosition, Quaternion.identity, _agentsParent);
                    Agent unit = spawnedUnit.GetComponent<Agent>();
                    unit.PathRequestManager = pathRequestManager;
                    unit.AgentsManager = this;
                    unit.Target = target;
                    _units.Add(unit);

                    _agentsCount--;
                    if (_agentsCount <= 0)
                        break;
                }

                if (_agentsCount <= 0)
                    break;
            }
        }

        public Node GetRandomWalkableNode()
        {
            int x;
            int y;
            Node node = null;

            do
            {
                x = Random.Range(0, grid.NodesCountX);
                y = Random.Range(0, grid.NodesCountY);
                node = grid.Nodes[x,y];
            }while(!node.IsWalkable);

            return node;
        }

        GameObject GetUnitObject()
        {
            GameObject unit = _agents[_currentUnitObjectIndex];
            _currentUnitObjectIndex = (_currentUnitObjectIndex + 1) % _agents.Count();
            return unit;
        }
    }
}
