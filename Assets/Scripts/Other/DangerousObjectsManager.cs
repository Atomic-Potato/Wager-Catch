using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DangerousObjectsManager : Singleton<DangerousObjectsManager>
{
    List<IDangerousObject> _spawnedObjects = new List<IDangerousObject>();
    public List<IDangerousObject> SpawnedObjects => _spawnedObjects;

    public void DestroyAllObjects()
    {
        List<IDangerousObject> objects = new List<IDangerousObject>(_spawnedObjects);
        foreach (IDangerousObject o in objects)
            o.DestroySelf();
        _spawnedObjects.Clear();
    }
}
