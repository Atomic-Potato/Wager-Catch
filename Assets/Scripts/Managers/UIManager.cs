using TMPro;
using UnityEngine;
using System;
using Ability;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Space, Header("Team Selection UI")]
    [Space, SerializeField] GameObject _teamSelectionUIParent;
    [SerializeField] TMP_Text _teamSelectionUIBalanceText;
    [SerializeField] TMP_Text _wagerText;
    [SerializeField] Slider _slider;

    [Space, SerializeField] RectTransform _runnersList;
    [SerializeField] RunnerStatsDisplay _runnerStatsDisplayPrefab;
    [SerializeField] TMP_Text _runnersWinText;
    [SerializeField] TMP_Text _runnersLossText;
    [SerializeField] Button _runnersBetButton;

    [Space, SerializeField] RectTransform _catchersList;
    [SerializeField] CatcherStatsDisplay _catcherStatsDisplayPrefab;
    [SerializeField] TMP_Text _catchersWinText;
    [SerializeField] TMP_Text _catchersLossText;
    [SerializeField] Button _catchersBetButton;
    

    [Space, Header("In Game UI")]
    [Space, SerializeField] GameObject _inGameUIParent;
    [SerializeField] TMP_Text _gameTimerText;
    [SerializeField] TMP_Text _balanceText;
    [SerializeField] TMP_Text _profitText;
    [SerializeField] RectTransform _abilityItemsListParent;
    [SerializeField] TMP_Text _yourTeamText;

    [Space, Header("Other")]
    [SerializeField] UI _defaultScreen;

    public enum UI
    {
        TeamSelection,
        InGame,
    }

    UI _currentScreen;
    public UI CurrentScreen => _currentScreen;

    new void Awake()
    {
        base.Awake();
        SetScreen(_defaultScreen);
        
         Debug.Log( GameManager.Instance.PlayerTeam.ToString());
    }

    void Start()
    {
        GameManager.Instance.BalanceChangeBroadcaster.AddListener(UpdateBalanceText);
        GameManager.Instance.MatchTimeBroadcaster.AddListener(UpdateGameTimer);
        _runnersBetButton.onClick.AddListener(GameManager.Instance.BetOnRunners);
        _catchersBetButton.onClick.AddListener(GameManager.Instance.BetOnCatchers);
        UpdateBalanceText();
        LoadAbilityItems();
        LoadPlayersStatsList();
    }

    public static string ConvertIntToShortMoney(int money)
    {
        if (money / 1000f < 1f)
            return money.ToString() + "$";
        else if (money / 1000000f < 1f)
            return (money/1000).ToString() + "K$";
        else
            return (money/1000000).ToString() + "M$";
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
            case UI.InGame:
                UpdatePlayerTeamText();
                UpdateBalanceText();
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

    #region SELECTING A WAGER
    int _selectedWager;
    public int SelectedWager => _selectedWager;
    public void UpdateWager()
    {
        int balance = GameManager.Instance.Balance;
        float percentage = _slider.value;
        _selectedWager = (int)(balance * percentage);
        

        _teamSelectionUIBalanceText.text = "Balance:\n" + ConvertIntToShortMoney(balance - _selectedWager);
        _wagerText.text = "Wager:\n" + (int)(percentage * 100) + "% " + ConvertIntToShortMoney(_selectedWager);
    }
    #endregion

    void UpdatePlayerTeamText()
    {
        _yourTeamText.color = GameManager.Instance.PlayerTeamColor;
        _yourTeamText.text = GameManager.Instance.PlayerTeam.ToString();
    }

    void LoadPlayersStatsList()
    {
        TeamsManager teamsManager = TeamsManager.Instance;
        LoadRunnersStatsList();
        LoadCatchersStatsList();

        void LoadRunnersStatsList()
        {
            foreach (RunnerStats runnerStats in teamsManager.GetRunnersStatsList())
            {
                RunnerStatsDisplay runnerDisplayItem = Instantiate(_runnerStatsDisplayPrefab, _runnersList);
                runnerDisplayItem.SetSpeedValue((runnerStats.Speed / runnerStats.MaxSpeed) * 100f);
                runnerDisplayItem.SetStaminaValue((runnerStats.Stamina / runnerStats.MaxStamina) * 100f);
            }
        }

        void LoadCatchersStatsList()
        {
            foreach (CatcherStats catcherStats in teamsManager.GetCatchersStatsList())
            {
                CatcherStatsDisplay runnerDisplayItem = Instantiate(_catcherStatsDisplayPrefab, _catchersList);
                runnerDisplayItem.SetSpeedValue((catcherStats.Speed / catcherStats.MaxSpeed) * 100f);
                runnerDisplayItem.SetCatchRangeValue((catcherStats.CatchRange / catcherStats.MaxCatchRange) * 100f);
            }
        }
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
        int profit = GameManager.Instance.Profit;
        _balanceText.text = ConvertIntToShortMoney(balance);
        _profitText.text = "PROFIT: " + ConvertIntToShortMoney(profit);
    }

    void UpdateTeamSelectionUIText()
    {
        GameManager gameManager = GameManager.Instance;
        _teamSelectionUIBalanceText.text = "Balance:\n" + ConvertIntToShortMoney(gameManager.Balance);
        _wagerText.text = "Wager:\n0% 0$";
        _runnersWinText.text = "WIN: " + gameManager.RunnersProfitPercentage + "%";
        _runnersLossText.text = "LOSS: " + gameManager.RunnersLossPercentage + "%";
        _catchersWinText.text = "WIN: " + gameManager.CatchersProfitPercentage + "%";
        _catchersLossText.text = "LOSS: " + gameManager.CatchersLossPercentage + "%";
    }
}
