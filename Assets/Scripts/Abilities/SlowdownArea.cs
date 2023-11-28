using UnityEngine;

namespace Abilities
{
    public class SlowdownArea : MonoBehaviour, IAbility
    {
        [SerializeField, Range(0f, 1f)] float _slowdownFactor = 0.5f;
        [SerializeField, Min(0f)] float __timeUntilSelfDestruct = 1f;

            Coroutine _selfDestructCoroutine;

        public void Destroy()
        {
            GridPlacementManager.Instance.RemoveObject(gameObject);
            Object.Destroy(gameObject);
        }

        public void Spawn()
        {
            
        }
    }
}
