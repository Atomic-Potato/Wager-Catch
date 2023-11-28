using System.Collections;
using Abilities;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Barrier : MonoBehaviour, IAbility
{
    [SerializeField] float _timeUntilSelfDestruct = 1f;

    Coroutine _selfDestructCoroutine;

    public void Destroy()
    {
        GridPlacementManager.Instance.RemoveObject(gameObject);
        Object.Destroy(gameObject);
    }

    public void Spawn()
    {
        if (_selfDestructCoroutine == null)
            _selfDestructCoroutine = StartCoroutine(SelfDestruct());
    }

    void OnEnable()
    {
        Spawn();
    }

    IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(_timeUntilSelfDestruct);
        Destroy();
    }
}
