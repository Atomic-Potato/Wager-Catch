using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    public class TestUnitsManager : MonoBehaviour
    {
        [SerializeField, Min(0)] int unitsCount;
        [SerializeField] GameObject[] testUnits;
        [SerializeField] Transform testUnitParent;
        [SerializeField] Grid grid;
        [SerializeField] PathRequestManager pathRequestManager;

        List<TeamPlayer> _units = new List<TeamPlayer>();
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
                    GameObject spawnedUnit = Instantiate(unitObject, grid.Nodes[i,j].WorldPosition, Quaternion.identity, testUnitParent);
                    TeamPlayer unit = spawnedUnit.GetComponent<TeamPlayer>();
                    unit.PathRequestManager = pathRequestManager;
                    _units.Add(unit);

                    unitsCount--;
                    if (unitsCount <= 0)
                        break;
                }

                if (unitsCount <= 0)
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
            GameObject unit = testUnits[_currentUnitObjectIndex];
            _currentUnitObjectIndex = (_currentUnitObjectIndex + 1) % testUnits.Count();
            return unit;
        }
    }
}
