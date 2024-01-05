using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ability
{
    public class AbilitiesManager : Singleton<AbilitiesManager>
    {
        [SerializeField] List<AbilityItem> _abilityItems = new List<AbilityItem>();
        public List<AbilityItem> AbilityItems => _abilityItems;
        
        [HideInInspector] public AbilityItem SelectedAbilityItem;

        new void Awake()
        {
            base.Awake();
            _abilityItems = _abilityItems.OrderBy(item => item.Cost).ToList();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && SelectedAbilityItem != null)
            {
                if (SelectedAbilityItem.IsCanBeConsumed)
                {
                    bool isConsumed = SelectedAbilityItem.Consume();
                    if (isConsumed)
                    {
                        GameManager.Instance.DeductBalance(SelectedAbilityItem.Cost);
                        // RemoveAbilitySelection();
                    }
                }
            }
        }

        void RemoveAbilitySelection()
        {
            SelectedAbilityItem = null;
            GridPlacementManager.Instance.RemovePreviewSprite();
        }
    }

}