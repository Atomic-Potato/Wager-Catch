﻿using System.Collections;
using UnityEngine;

namespace Abilities
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        [SerializeField, Min(0f)] float _duration = 1f;
        Coroutine _selfDestructCoroutine;

        public virtual void Destroy()
        {
            GridPlacementManager.Instance.RemoveObject(gameObject);
            Object.Destroy(gameObject);
        }

        public virtual void Spawn()
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
            yield return new WaitForSeconds(_duration);
            Destroy();
        }
    }
}
