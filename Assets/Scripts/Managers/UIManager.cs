using TMPro;
using UnityEngine;
using System;
using Ability;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text _gameTimerText;
    [SerializeField] TMP_Text _balanceText;
    [SerializeField] RectTransform _abilityItemsListParent;

    void Start()
    {
        GameManager.Instance.BalanceChangeBroadcaster.AddListener(UpdateBalanceText);
        GameManager.Instance.MatchTimeBroadcaster.AddListener(UpdateGameTimer);
        UpdateBalanceText();
        LoadAbilityItems();
    }

    void UpdateGameTimer()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.MatchTimeLeftInSeconds);
        
        if (timeSpan.Minutes > 0)
        {
            _gameTimerText.text = 
                timeSpan.Minutes.ToString("0") + ":" +
                timeSpan.Seconds.ToString("00");
        }
        else
        {
            _gameTimerText.text = timeSpan.Seconds.ToString("0");
        }
            
    }

    void LoadAbilityItems()
    {
        foreach (AbilityItem item in AbilitiesManager.Instance.AbilityItems)
            Instantiate(item.gameObject, _abilityItemsListParent);
    }

    void UpdateBalanceText()
    {
        int balance = GameManager.Instance.Balance;
        _balanceText.text = balance + "$";
    }
}
