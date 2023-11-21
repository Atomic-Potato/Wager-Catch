using UnityEngine;
using Pathfinding;

public class GridPlacementManager : MonoBehaviour
{
    [SerializeField] Pathfinding.Grid placementGrid;
    [SerializeField] Pathfinding.Grid unitsGrid;
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] LayerMask unitsLayer;

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
            BoxCollider2D toSpawnCollider = objectToSpawn.GetComponent<BoxCollider2D>();
            bool isAgentOnNode = Physics2D.OverlapBox(placementNode.WorldPosition, toSpawnCollider.size, 0f, unitsLayer) != null;
            return !placementNode.IsSafe && placementNode.IsWalkable && placementNode.IsEditable && !isAgentOnNode;
        }
    }

}
