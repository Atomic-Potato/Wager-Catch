using System.Collections;
using System.Collections.Generic;
using Abilities;
using UnityEngine;

public class AbilitiesManager : Singleton<AbilitiesManager>
{
    [SerializeField] GameObject _abilityPrefab;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AbilityBase ability = _abilityPrefab.GetComponent<AbilityBase>();

            if (ability.IsCanBeUsedAnywhere)
                GridPlacementManager.Instance.PlaceObjectAnywhere(_abilityPrefab);
            else
                GridPlacementManager.Instance.PlaceObjectAnywhere(_abilityPrefab);
        }
    }
}
