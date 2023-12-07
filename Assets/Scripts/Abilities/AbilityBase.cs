using System.Collections;
using UnityEngine;

namespace Ability
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        [SerializeField, Min(0f)] float _duration = 1f;
        [SerializeField] bool _isEndlessDuration;
        public bool IsCanBeUsedAnywhere;
        [HideInInspector] public AbilityItem Item;
        [Space]
        
        Coroutine _selfDestructCoroutine;

        public virtual void Destroy()
        {
            GridPlacementManager.Instance.RemoveObject(gameObject);
            Object.Destroy(gameObject);
        }

        public virtual void Spawn()
        {
            if (!_isEndlessDuration && _selfDestructCoroutine == null)
                _selfDestructCoroutine = StartCoroutine(SelfDestruct());
            Item.Consume();
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
