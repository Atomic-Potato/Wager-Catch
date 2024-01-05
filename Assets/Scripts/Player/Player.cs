using Pathfinding;
using UnityEngine;

public class Player : UnitBase, IDangerousObject
{
    [Space, Header("Player Settings")]
    [SerializeField] Collider2D _collider;

    bool _isActive = true;
    bool _isUserInControl;

    new void Start()
    {
        base.Start();
        _target = GameManager.Instance.PlayerEntryPoint.position;
        SendPathRequest();
    }

    new void Update()
    {
        if (!_isActive)
            return;

        base.Update();

        if (!_isUserInControl && _isReachedDestination)
        {
            _isUserInControl = true;
            AddToDangerousObjectsManager();
        }
        
        if (_isUserInControl)
            Move();
    }

    void Move()
    {
        Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        _rigidbody.velocity = inputDirection * _speed;
        _isMoving = inputDirection != Vector2.zero; 
    }

    public void AddToDangerousObjectsManager()
    {
        DangerousObjectsManager.Instance.SpawnedObjects.Add(this);
    }

    public void Deactivate()
    {
        _isActive = false;
        _isMoving = false;
        _collider.enabled = false;
        _rigidbody.velocity = Vector2.zero;
        Destroy(_rigidbody);
    }

    public void DestroySelf()
    {
        GameManager.Instance.PlayerInstance = null;
        UIManager.Instance.ShowAbilitiesList();
        gameObject.SetActive(false);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool IsDeactivated()
    {
        return !_isActive;
    }

    public void SetLocalPosition(Vector2 position)
    {
        transform.localPosition = position;
    }

    public void SetParent(Transform parent)
    {
        transform.parent = parent;
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    
}
