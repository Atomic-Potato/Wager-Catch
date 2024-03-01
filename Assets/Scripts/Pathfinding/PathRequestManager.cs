using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Pathfinding
{
    public class PathRequestManager : Singleton<PathRequestManager>
    {
        [SerializeField] Pathfinder _pathfinder;
        Queue<PathRequestResult> _results = new Queue<PathRequestResult>();

        void Update()
        {
            if (_results.Count == 0)
                return;

            lock (_results)
            {
                while (_results.Count > 0)
                {
                    PathRequestResult result = _results.Dequeue();
                    result.Callback(result.Path, result.IsSuccess, result.EndNodeCache);
                }
            }
        }

        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                Instance._pathfinder.FindPathOnGrid(request, Instance.FinishProcessingPathRequest);
            };
            threadStart.Invoke();
        }

        void FinishProcessingPathRequest(PathRequestResult result)
        {
            lock (_results)
            {
                _results.Enqueue(result);
            }
        }

    }

    public struct PathRequestResult
    {
        public Vector2[] Path;
        public bool IsSuccess;
        public Node EndNodeCache;
        public Action<Vector2[], bool, Node> Callback;

        public PathRequestResult(Vector2[] path, bool isSuccess, Node endNodeCache, Action<Vector2[], bool, Node> callback)
        {
            this.Path = path;
            this.IsSuccess = isSuccess;
            this.EndNodeCache = endNodeCache;
            this.Callback = callback;
        }
    }

    public struct PathRequest
    {
        public Vector2 StartPosition;
        public Vector3 EndPosition;
        public Grid Grid;
        public Node EndNodeCache;
        public Action<Vector2[], bool, Node> Callback; // this will be called once the path is returned

        public PathRequest(Vector2 startPosition, Vector2 endPosition, Grid grid, Node endNodeCache, Action<Vector2[], bool, Node> callback)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Grid = grid;
            EndNodeCache = endNodeCache;
            Callback = callback;
        }
    }
}