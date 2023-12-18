using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Ability
{
    public class AbilityItem : MonoBehaviour
    {
        [SerializeField, Min(0f)] int _cost;
        public int Cost => _cost;

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
            Ability.Item = this;

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
            {
                AbilitiesManager.Instance.SelectedAbilityItem = this;
                GridPlacementManager.Instance.SetPreviewSprite(_iconActive.GetComponent<Image>().sprite);
            }
        }

        /// <summary>
        /// Activates the ability
        /// </summary>
        /// <returns>If the ability was activated</returns>
        public bool Consume()
        {
            if (_numberOfUses <= 0)
                return false;

            bool isActived = ActivateAbility();
            if (!isActived)
                return false;

            _usesLeft--;
            UpdateUIText();
            if (_usesLeft <= 0)
                DecativateIcon();

            return true;

            bool ActivateAbility()
            {
                if (Prefab.GetComponent<PlayerSpawner>() != null && GameManager.Instance.PlayerInstance != null)
                    return false;

                if (Ability.IsCanBeUsedAnywhere)
                    return GridPlacementManager.Instance.PlaceObjectAnywhere(Prefab);
                else
                    return GridPlacementManager.Instance.PlaceObject(Prefab);
            }
        }

        public void Restore()
        {
            if (_usesLeft >= _numberOfUses)
                return;
            _usesLeft++;
            UpdateUIText();
            if (_usesLeft > 0)
                ActivateIcon();
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
