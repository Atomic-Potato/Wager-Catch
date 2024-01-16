using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ability
{
    public class AbilityItem : MonoBehaviour
    {
        #region Global Variables
        [SerializeField, Min(0f)] int _cost;
        public int Cost => _cost;

        [Tooltip("-1: Infinite number of uses")]
        [SerializeField, Min(-1)] int _numberOfUses = -1;
        [SerializeField] UnityEngine.GameObject _abilityPrefab;
        
        [Space]
        [SerializeField] Text _usesText;
        
        [Space]
        [SerializeField] TMP_Text _costText;
        [SerializeField] Color _cheapColor = new Color(0f, 0.490566f, 0f, 1f);
        [SerializeField] Color _expensiveColor = new Color(1f, 0f, 0f, 1f);
        
        [Space]
        [SerializeField] GameObject _iconActive;
        [SerializeField] GameObject _iconInactive;
        
        int _usesLeft;
    
        public int NumberOfUses => _numberOfUses; 
        public AbilityBase Ability  => _abilityPrefab.GetComponent<AbilityBase>();
        public UnityEngine.GameObject Prefab => _abilityPrefab;
        public bool IsUnlimited => _numberOfUses < 0;
        public bool IsActive => IsUnlimited || _usesLeft > 0;
        public bool IsCanBeConsumed => (IsUnlimited || _usesLeft > 0) && _cost <= GameManager.Instance.Balance;
        #endregion

        void OnEnable()
        {
            _usesLeft = _numberOfUses;
            Ability.Item = this;

            _costText.text =  _cost >= 1000 ? (_cost/1000).ToString()+"K$" : _cost+ "$";
            UpdateCostColor();
            GameManager.Instance.BalanceChangeBroadcaster.AddListener(UpdateCostColor);

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
                AbilitiesManager.Instance.SelectAbilityItem(this);
                GridPlacementManager.Instance.SetPreviewSprite(_iconActive.GetComponent<Image>().sprite);
            }
        }

        /// <summary>
        /// Activates the ability
        /// </summary>
        /// <returns>If the ability was activated</returns>
        public bool Consume()
        {
            if (_numberOfUses <= 0 && !IsUnlimited)
                return false;

            bool isActived = ActivateAbility();
            if (!isActived)
                return false;

            _usesLeft--;
            UpdateUIText();
            if (_usesLeft <= 0 && !IsUnlimited)
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

        void UpdateCostColor()
        {
            _costText.color = _cost <= GameManager.Instance.Balance ? _cheapColor : _expensiveColor;
        }
    }
}
