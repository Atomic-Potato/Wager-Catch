﻿using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using System;
public class GridPlacementManager : Singleton<GridPlacementManager>
{
    [SerializeField] Pathfinding.Grid _placementGrid;
    [SerializeField] Pathfinding.Grid _unitsGrid;
    [SerializeField] GameObject _objectToSpawn;
    [SerializeField] LayerMask _invalidPlacementLayers;

    [SerializeField] TagsManager.Tag _noCollisionTags;
    public List<GameObject> _placedObjects = new List<GameObject>(); 

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            PlaceObject(_objectToSpawn);
    }

    /// <summary>
    /// Instantiates an object on the grid
    /// </summary>
    /// <param name="placedObjectPrefab"></param>
    /// <returns>If it was successful, i.e. object was placed</returns>
    public bool PlaceObject(GameObject placedObjectPrefab)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Node placementNode = _placementGrid.GetNodeFromWorldPosition(mousePosition);

        if (!IsCanPlaceOnNode())
            return false;

        GameObject spawnedObject = Instantiate(placedObjectPrefab, placementNode.WorldPosition, Quaternion.identity);
        BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();
        _placedObjects.Add(spawnedObject);

        if (collider != null && !collider.isTrigger && IsCollisionTag())
        {
            _unitsGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
            _placementGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
        }
        
        return true;

        bool IsCanPlaceOnNode()
        {
            BoxCollider2D toSpawnCollider = placedObjectPrefab.GetComponent<BoxCollider2D>();
            bool isAgentOnNode = Physics2D.OverlapBox(placementNode.WorldPosition, toSpawnCollider.size, 0f, _invalidPlacementLayers) != null;
            return !placementNode.IsSafe && placementNode.IsWalkable && placementNode.IsEditable && !isAgentOnNode;
        }

        bool IsCollisionTag()
        {
            TagsManager.Tag colliderTag = TagsManager.GetTagFromString(collider.gameObject.tag);
            return TagsManager.IsTagOneOfMultipleTags(colliderTag, _noCollisionTags);
        }
    }

    /// <summary>
    /// Instantiates an object on the grid
    /// </summary>
    /// <param name="placedObjectPrefab"></param>
    /// <returns>If it was successful, i.e. object was placed</returns>
    public bool PlaceObjectAnywhere(GameObject placedObjectPrefab)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Node placementNode = _placementGrid.GetNodeFromWorldPosition(mousePosition);

        GameObject spawnedObject = Instantiate(placedObjectPrefab, placementNode.WorldPosition, Quaternion.identity);
        BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();
        _placedObjects.Add(spawnedObject);

        if (collider != null && !collider.isTrigger && IsCollisionTag())
        {
            _unitsGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
            _placementGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
        }
        
        return true;

        bool IsCollisionTag()
        {
            TagsManager.Tag colliderTag = TagsManager.GetTagFromString(collider.gameObject.tag);
            return TagsManager.IsTagOneOfMultipleTags(colliderTag, _noCollisionTags);
        }
    }

    public void RemoveObject(GameObject removedObject)
    {
        if (!_placedObjects.Contains(removedObject))
            return;

        BoxCollider2D collider = removedObject.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Node placementNode = _placementGrid.GetNodeFromWorldPosition(removedObject.transform.position);
            Bounds bounds = collider.bounds;

            collider.enabled = false;

            _unitsGrid.UpdateGridSection(bounds.min, bounds.max, placementNode.IsSafe, true);
            _placementGrid.UpdateGridSection(bounds.min, bounds.max, placementNode.IsSafe, true);
        }
        
        _placedObjects.Remove(removedObject);
        
    }

}
