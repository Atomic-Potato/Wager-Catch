using UnityEngine;
using Pathfinding;

public class GridPlacementManager : MonoBehaviour
{
    [SerializeField] Pathfinding.Grid _placementGrid;
    [SerializeField] Pathfinding.Grid _unitsGrid;
    [SerializeField] GameObject _objectToSpawn;
    [SerializeField] LayerMask _invalidPlacementLayers;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            PlaceObject();
    }

    void PlaceObject()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Node placementNode = _placementGrid.GetNodeFromWorldPosition(mousePosition);

        if (!IsCanPlaceOnNode())
            return;

        GameObject spawnedObject = Instantiate(_objectToSpawn, placementNode.WorldPosition, Quaternion.identity);
        BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            _unitsGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
            _placementGrid.UpdateGridSection(collider.bounds.min, collider.bounds.max, placementNode.IsSafe, false);
        }
        
        bool IsCanPlaceOnNode()
        {
            BoxCollider2D toSpawnCollider = _objectToSpawn.GetComponent<BoxCollider2D>();
            bool isAgentOnNode = Physics2D.OverlapBox(placementNode.WorldPosition, toSpawnCollider.size, 0f, _invalidPlacementLayers) != null;
            return !placementNode.IsSafe && placementNode.IsWalkable && placementNode.IsEditable && !isAgentOnNode;
        }
    }

}
