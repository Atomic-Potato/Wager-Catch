using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class GridsManager : Singleton<GridsManager>
    {
        static int _globalMaxSize;
        public static int GlobalMaxSize => _globalMaxSize;
        
        [SerializeField] Grid _gridA;
        public Grid GridA => _gridA;
        [SerializeField] Grid _gridB;
        public Grid GridB => _gridB;
        [SerializeField] Grid _gridC;
        public Grid GridC => _gridC;
        [SerializeField] Grid _gridD;
        public Grid GridD => _gridD;
        [SerializeField] Grid _gridE;
        public Grid GridE => _gridE;

        // Each grid will add itself to this list
        List<Grid> _grids = new List<Grid>();
        public List<Grid> Grids => _grids;

        new void Awake()
        {
            base.Awake();
            // Note: 
            //      I would have made each grid add itself, 
            //      but then i would need to set the global max size in Start
            //      and Pathfinders requires it in awake
            //      and messing with execution orders doesnt always work in builds 
            _grids = new List<Grid> {_gridA, _gridB, _gridC, _gridD, _gridE};
            _globalMaxSize = GetMaxGridSize();

        }

        public Grid GetGrid(Agent.Type type)
        {
            switch (type)
            {
                case Agent.Type.A:
                    if (_gridA == null)
                        throw new System.Exception("No grid set for agent of type " + type);
                    return _gridA;
                case Agent.Type.B:
                    if (_gridB == null)
                        throw new System.Exception("No grid set for agent of type " + type);
                    return _gridB;
                case Agent.Type.C:
                    if (_gridC == null)
                        throw new System.Exception("No grid set for agent of type " + type);
                    return _gridC;
                case Agent.Type.D:
                    if (_gridD == null)
                        throw new System.Exception("No grid set for agent of type " + type);
                    return _gridD;
                case Agent.Type.E:
                    if (_gridE == null)
                        throw new System.Exception("No grid set for agent of type " + type);
                    return _gridD;
                default:
                    throw new System.Exception("No grid set for agent of type " + type);
            }
        }
        
        int GetMaxGridSize()
        {
            int maxSize = 0;
                foreach(Grid grid in _grids)
                {
                    if (grid == null)
                        continue;
                    if (grid.MaxSize > maxSize)
                        maxSize = grid.MaxSize;
                }
            return maxSize;
        }
    }
}
