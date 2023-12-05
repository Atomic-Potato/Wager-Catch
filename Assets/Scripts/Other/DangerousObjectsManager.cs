using System.Collections.Generic;
public class DangerousObjectsManager : Singleton<DangerousObjectsManager>
{
    List<IDangerousObject> _spawnedObjects = new List<IDangerousObject>();
    public List<IDangerousObject> SpawnedObjects => _spawnedObjects;

    public void DestroyAllObjects()
    {
        List<IDangerousObject> objetcs = new List<IDangerousObject>(_spawnedObjects);
        foreach (IDangerousObject o in _spawnedObjects)
            o.DestroySelf();
        _spawnedObjects.Clear();
    }
}
