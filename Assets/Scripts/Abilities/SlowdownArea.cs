using UnityEngine;

namespace Ability
{
    public class SlowdownArea : AbilityBase
    {
        [SerializeField, Range(0f, 1f)] float _slowdownFactor = 0.5f;
        public float SlowdownFactor => _slowdownFactor;
    }
}
