using UnityEngine;
using Pathfinding;
using Unity.Mathematics;

public class GridPlacementManager : MonoBehaviour
{
    [SerializeField] Pathfinding.Grid placementGrid;
    [SerializeField] Pathfinding.Grid unitsGrid;
    [SerializeField] GameObject objectToSpawn;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            PlaceObject();
    }

    void PlaceObject()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Node placementNode = placementGrid.GetNodeFromWorldPosition(mousePosition);

        if (!IsCanPlaceOnNode())
            return;

        GameObject spawnedObject = Instantiate(objectToSpawn, placementNode.WorldPosition, Quaternion.identity);
        BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            unitsGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
            placementGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
        }
        
        bool IsCanPlaceOnNode()
        {
            return !placementNode.IsSafe && placementNode.IsWalkable && placementNode.IsEditable;
        }
    }

}
