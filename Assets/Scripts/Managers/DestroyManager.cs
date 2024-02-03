using System.Collections;
using UnityEngine;

public class DestroyManager : Singleton<DestroyManager>
{
    public void Destroy(Object obj, float delay = 0, bool isRealTimeDelay = false)
    {
        if (isRealTimeDelay)
            StartCoroutine(DestroyRealTime(obj, delay));
        else
            Destroy(obj, delay);
    }

    IEnumerator DestroyRealTime(Object obj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        GameObject.Destroy(obj); 
    }
}
