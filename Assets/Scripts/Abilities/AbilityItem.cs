using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Ability
{
    public class AbilityItem : MonoBehaviour
    {
        [Tooltip("-1: Infinite number of uses")]
        [SerializeField, Min(-1)] int _numberOfUses = -1;
        [SerializeField] UnityEngine.GameObject _abilityPrefab;
        [SerializeField] Text _usesText;
        [SerializeField] GameObject _iconActive;
        [SerializeField] GameObject _iconInactive;
        
        int _usesLeft;
    
        public int NumberOfUses => _numberOfUses; 
        public AbilityBase Ability  => _abilityPrefab.GetComponent<AbilityBase>();
        public UnityEngine.GameObject Prefab => _abilityPrefab;
        public bool IsUnlimited => _numberOfUses < 0;
        public bool IsActive => IsUnlimited || _usesLeft > 0;
        public bool IsCanBeConsumed => IsUnlimited || _usesLeft > 0;
        
        void OnEnable()
        {
            _usesLeft = _numberOfUses;

            if (!IsCanBeConsumed)
                DecativateIcon();

            if (IsUnlimited)
                _usesText.text = "∞";
            else
                _usesText.text = _usesLeft.ToString();
        }

        public void Select()
        {
            if (IsCanBeConsumed)
                AbilitiesManager.Instance.SelectedAbilityItem = this;
        }

        public void Consume()
        {
            if (_numberOfUses <= 0)
                return;

            _usesLeft--;
            UpdateUIText();
            if (_usesLeft <= 0)
                DecativateIcon();
        }

        void UpdateUIText()
        {
            if (IsUnlimited)
                return;
            _usesText.text = _usesLeft.ToString();
        }

        void DecativateIcon()
        {
            _iconActive.SetActive(false);
            _iconInactive.SetActive(true);
        }

        void ActivateIcon()
        {
            _iconActive.SetActive(true);
            _iconInactive.SetActive(false);
        }
    }
}
