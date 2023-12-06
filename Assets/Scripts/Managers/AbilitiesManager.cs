using System.Collections.Generic;
using UnityEngine;

namespace Ability
{
    public class AbilitiesManager : Singleton<AbilitiesManager>
    {
        [SerializeField] List<AbilityItem> _abilityItems = new List<AbilityItem>();
        public List<AbilityItem> AbilityItems => _abilityItems;
        
        [HideInInspector] public AbilityItem SelectedAbilityItem;

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && SelectedAbilityItem != null)
            {
                if (SelectedAbilityItem.IsCanBeConsumed)
                {
                    bool isWasPlaced;
                    
                    if (SelectedAbilityItem.Ability.IsCanBeUsedAnywhere)
                        isWasPlaced = GridPlacementManager.Instance.PlaceObjectAnywhere(SelectedAbilityItem.Prefab);
                    else
                        isWasPlaced = GridPlacementManager.Instance.PlaceObject(SelectedAbilityItem.Prefab);
                    
                    if (isWasPlaced)
                        SelectedAbilityItem.Consume();
                }
                else
                {
                    SelectedAbilityItem = null;
                }
            }
        }

    }

}