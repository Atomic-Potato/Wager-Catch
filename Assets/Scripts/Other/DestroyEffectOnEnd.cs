using System.Collections;
using UnityEngine;
public class DestroyEffectOnEnd : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    void Awake()
    {
        StartCoroutine(DelayDestroy());
    }
    
    IEnumerator DelayDestroy()
    {
        float particlesDuration = _particleSystem.main.duration;
        yield return new WaitForSeconds(particlesDuration);
        Destroy(gameObject);
    }
}
