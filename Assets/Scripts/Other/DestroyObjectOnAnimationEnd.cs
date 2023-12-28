using System.Collections;
using UnityEngine;

public class DestroyObjectOnAnimationEnd : MonoBehaviour
{
    [SerializeField] AnimationClip _animationClip;
    void Awake()
    {
        StartCoroutine(DelayDestroy());
    }
    
    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(_animationClip.length);
        Destroy(gameObject);
    }
}
