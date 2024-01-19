using Ability;
using UnityEngine;

public class NukeManager : MonoBehaviour
{
    [SerializeField] AbilityItem _nukeItem;
    [SerializeField] GameObject _nukeEffectsPrefab;

    GameObject _nukeEffects;
    bool _isUseEffects;
    

    void Awake()
    {
        _nukeEffects = Instantiate(_nukeEffectsPrefab, Vector2.zero, Quaternion.identity);
        _nukeEffects.SetActive(false);
    }

    void Start()
    {
        _nukeItem.AbilitySelectedBroadcaster.AddListener(EnableNukeEffects);
        _nukeItem.AbilityDeselectedBrodcaster.AddListener(DisableNukeEffects);
    }

    void EnableNukeEffects()
    {
        if (!_nukeItem.IsSelected)
            return;
        _isUseEffects = true;
        _nukeEffects.SetActive(true);
    }

    void DisableNukeEffects()
    {
        _isUseEffects = false;
        _nukeEffects.SetActive(false);
    }

    
}
