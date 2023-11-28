using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    [SerializeField] bool _isSingleton;
    [SerializeField] bool _isRemoveParent;
    static HashSet<string> _spawnedObjects = new HashSet<string>();

    void Start()
    {
        if (_isSingleton)
        {
            if (_spawnedObjects.Contains(name))
            {
                Destroy(this);
                return;
            }
                
            _spawnedObjects.Add(name);
        }

        if (_isRemoveParent)
            transform.SetParent(null);
        
        DontDestroyOnLoad(gameObject);
    }
}
