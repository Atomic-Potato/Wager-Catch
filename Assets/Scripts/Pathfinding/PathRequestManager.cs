using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class PathRequestManager : Singleton<PathRequestManager>
    {
        [SerializeField] Pathfinder pathfinder;

        Queue<PathRequest> _pathRequestsQueue = new Queue<PathRequest>();
        PathRequest _currentPathRequest;
        bool _isProcessingPath;

        public void RequestPath(Vector2 startPosition, Vector2 endPosition, Grid grid,  Node endNodeCache, Action<Vector2[], bool, Node> callback)
        {
            PathRequest pathRequest = new PathRequest(startPosition, endPosition, grid, endNodeCache, callback);
            _pathRequestsQueue.Enqueue(pathRequest);
            TryToProcessNextPathRequest();
        }

        void TryToProcessNextPathRequest()
        {
            if (_isProcessingPath || _pathRequestsQueue.Count == 0)
                return;
            
            _currentPathRequest = _pathRequestsQueue.Dequeue();
            _isProcessingPath = true;
            pathfinder.StartFindingPath(_currentPathRequest);
        }

        public void FinishProcessingPathRequest(Vector2[] path, bool isSuccess, Node endNodeCache)
        {
            _currentPathRequest.Callback(path, isSuccess, endNodeCache);
            _isProcessingPath = false;
            TryToProcessNextPathRequest();
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
