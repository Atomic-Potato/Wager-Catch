using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Ability
{
    public class AbilitiesManager : Singleton<AbilitiesManager>
    {
        [SerializeField, Range(0,1)] float _specialAbilitySpawnChance = .1f;

        [SerializeField] List<AbilityItem> _abilityItems = new List<AbilityItem>();
        public List<AbilityItem> AbilityItems => _abilityItems;
        
        AbilityItem _selectedAbilityItem;
        UnityEvent _abilitySelectionBroadcaster;
        UnityEvent _abilityRemovedBroadcaster;

        #region Execution
        new void Awake()
        {
            base.Awake();
            _abilityItems = _abilityItems.OrderBy(item => item.Cost).ToList();
            _abilitySelectionBroadcaster = new UnityEvent();
            _abilityRemovedBroadcaster = new UnityEvent();
            SpawnSpecialAbility();
        }

        void Update()
        {
            if (_selectedAbilityItem != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    RemoveAbilitySelection();
                }
                else 
                if (Input.GetMouseButtonDown(0))
                {
                    if (_selectedAbilityItem.IsCanBeConsumed)
                    {
                        bool isConsumed = _selectedAbilityItem.Consume();
                        if (isConsumed)
                        {
                            GameManager.Instance.DeductBalance(_selectedAbilityItem.Cost);
                            RemoveAbilitySelection();
                        }
                    }
                }
            }
        }
        #endregion

        public void AddAbilitySelectionBroadCasterListener(UnityAction call)
        {
            if (call == null)
                return;
            _abilitySelectionBroadcaster.AddListener(call);
        }

        public void AddAbilityRemovedBroadcasterListener(UnityAction call)
        {
            if (call == null)
                return;
            _abilityRemovedBroadcaster.AddListener(call);
        }

        public void SelectAbilityItem(AbilityItem abilityItem)
        {
            if (abilityItem == null)
                return;
            _selectedAbilityItem = abilityItem;
            _abilitySelectionBroadcaster.Invoke();
        }

        void RemoveAbilitySelection()
        {
            _selectedAbilityItem = null;
            GridPlacementManager.Instance.RemovePreviewSprite();
            _abilityRemovedBroadcaster.Invoke();
        }

        void SpawnSpecialAbility()
        {
            float res = UnityEngine.Random.Range(0f, 1f);
            Debug.Log(res);
            if (res <= _specialAbilitySpawnChance)
                UIManager.Instance.ShowSpecialAbility();
        }
    }

}