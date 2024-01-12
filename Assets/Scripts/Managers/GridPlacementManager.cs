using UnityEngine;
using Pathfinding;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;
public class GridPlacementManager : Singleton<GridPlacementManager>
{
    [SerializeField] Pathfinding.Grid _placementGrid;
    [SerializeField] Pathfinding.Grid _unitsGrid;
    [SerializeField] LayerMask _invalidPlacementLayers;
    [SerializeField] TagsManager.Tag _noCollisionTags;

    [Space, Header("Placement Preview")]
    [SerializeField] Color _invalidPlacementColor = new Color(1f, 0f, 0f);
    [SerializeField] SpriteRenderer _previewSpriteRenderer;
    [SerializeField] BoxCollider2D _previewCollider;
    [HideInInspector] public List<GameObject> PlacedObjects = new List<GameObject>(); 
    Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (_previewSpriteRenderer.sprite != null)
            ShowPreviewSprite();
    }

    void ShowPreviewSprite()
    {
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Node mouseNode = _placementGrid.GetNodeFromWorldPosition(mousePosition);
        _previewSpriteRenderer.color = !IsCanPlaceOnNode(mouseNode, _previewCollider) ? _invalidPlacementColor : Color.white; 
        _previewSpriteRenderer.gameObject.transform.position = mousePosition;
    }

    public void SetPreviewSprite(Sprite sprite)
    {
        _previewSpriteRenderer.sprite = sprite;
    }
    public void RemovePreviewSprite()
    {
        _previewSpriteRenderer.sprite = null;
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

        if (!IsCanPlaceOnNode(placementNode, placedObjectPrefab.GetComponent<BoxCollider2D>()))
            return false;

        GameObject spawnedObject = Instantiate(placedObjectPrefab, mousePosition, Quaternion.identity);
        BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();
        PlacedObjects.Add(spawnedObject);

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
    
    bool IsCanPlaceOnNode(Node node, BoxCollider2D toSpawnCollider)
    {
        // i dont wanna do dat no more
        // bool isAgentOnNode = Physics2D.OverlapBox(node.WorldPosition, toSpawnCollider.size, 0f, _invalidPlacementLayers) != null;
        // return !node.IsSafe && node.IsWalkable && node.IsEditable && !isAgentOnNode;
        return !node.IsSafe && node.IsWalkable;
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

        GameObject spawnedObject = Instantiate(placedObjectPrefab, mousePosition, Quaternion.identity);
        BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();
        PlacedObjects.Add(spawnedObject);

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
        if (!PlacedObjects.Contains(removedObject))
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
        
        PlacedObjects.Remove(removedObject);
        
    }

}
