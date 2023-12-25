using TMPro;
using UnityEngine;
using System;
using Ability;

public class UIManager : Singleton<UIManager>
{
    [Space, Header("Team Selection UI")]
    [Space, SerializeField] GameObject _teamSelectionUIParent;
    [SerializeField] TMP_Text _teamSelectionUIBalanceText;
    [SerializeField] TMP_Text _wagerText;
    [SerializeField] RectTransform _bettingSliderFill;
    [Space, SerializeField] RectTransform _runnersList;
    [SerializeField] TMP_Text _runnersWinText;
    [SerializeField] TMP_Text _runnersLossText;
    [Space, SerializeField] RectTransform _catchersList;
    [SerializeField] TMP_Text _catchersWinText;
    [SerializeField] TMP_Text _catchersLossText;
    

    [Space, Header("In Game UI")]
    [Space, SerializeField] GameObject _inGameUIParent;
    [SerializeField] TMP_Text _gameTimerText;
    [SerializeField] TMP_Text _balanceText;
    [SerializeField] RectTransform _abilityItemsListParent;

    [Space, Header("Other")]
    [SerializeField] UI _defaultScreen;

    public enum UI
    {
        TeamSelection,
        InGame,
    }

    UI _currentScreen;
    public UI CurrentScreen => _currentScreen;

    void Awake()
    {
        SetScreen(_defaultScreen);
    }

    void Start()
    {
        GameManager.Instance.BalanceChangeBroadcaster.AddListener(UpdateBalanceText);
        GameManager.Instance.MatchTimeBroadcaster.AddListener(UpdateGameTimer);
        UpdateBalanceText();
        LoadAbilityItems();
    }

    #region SCREEN MANAGEMENT
    public void SetScreen(UI screen)
    {
        DisableCurrentScreen();
        _currentScreen = screen;
        EnableCurrentScreen();
        UpdateScreen();
    }

    public void UpdateScreen()
    {
        switch (_currentScreen)
        {
            case UI.TeamSelection:
                UpdateTeamSelectionUIText();
                break;
        }
    }

    public void DisableCurrentScreen()
    {
        GetCurrentScreenParent().SetActive(false);
    }

    public void EnableCurrentScreen()
    {
        GetCurrentScreenParent().SetActive(true);
    }

    GameObject GetCurrentScreenParent()
    {
        switch (_currentScreen)
        {
            case UI.TeamSelection:
                return _teamSelectionUIParent;
            case UI.InGame:
                return _inGameUIParent;
            default:
                return null;
        }
    }
    #endregion

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

    void UpdateTeamSelectionUIText()
    {
        GameManager gameManager = GameManager.Instance;
        _teamSelectionUIBalanceText.text = "Balance:\n" + gameManager.Balance;
        _wagerText.text = "Wager:\n";
        _runnersWinText.text = "WIN: " + gameManager.RunnersProfitPercentage + "%";
        _runnersLossText.text = "LOSS: " + gameManager.RunnersLossPercentage + "%";
        _catchersWinText.text = "WIN: " + gameManager.CatchersProfitPercentage + "%";
        _catchersLossText.text = "LOSS: " + gameManager.CatchersLossPercentage + "%";
    }
}
