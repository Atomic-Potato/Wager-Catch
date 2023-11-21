﻿using System.Collections.Generic;
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

        List<Player> _units = new List<Player>();
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
                    Player unit = spawnedUnit.GetComponent<Player>();
                    unit.PathRequestManager = pathRequestManager;
                    unit.TestUnitsManager = this;
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
