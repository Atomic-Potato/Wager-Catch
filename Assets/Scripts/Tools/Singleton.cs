using UnityEngine;

[DefaultExecutionOrder(-1)]
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
#if UNITY_EDITOR
            if(_instance)
                return _instance;
            _instance = FindObjectOfType<T>();
            return _instance;
#else
            return _instance;
#endif
        }
    }

    protected void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        _instance = GetComponent<T>();
    }
}
