using UnityEngine;

public interface IDangerousObject
{
    void AddToDangerousObjectsManager();
    void Deactivate();
    bool IsDeactivated();
    void DestroySelf();
    Transform GetTransform();
    void SetParent(Transform parent);
    void SetPosition(Vector2 position);
    void SetLocalPosition(Vector2 position);
}
