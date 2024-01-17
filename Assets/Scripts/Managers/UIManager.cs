using TMPro;
using UnityEngine;
using System;
using Ability;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class UIManager : Singleton<UIManager>
{
    #region Global Variables
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
    [SerializeField] GameObject _abilitiesParent;
    [SerializeField] RectTransform _abilityItemsListParent;
    [SerializeField] GameObject _specialAbility;
    [SerializeField] TMP_Text _yourTeamText;

    [Space, Header("Match End UI")]
    [Space, SerializeField] GameObject _matchEndUIParent;
    [SerializeField] Button _matchEndButton;
    [SerializeField] TMP_Text _resultText;
    [SerializeField] TMP_Text _resultingProfitText;
    [SerializeField] TMP_Text _bonusText;

    [Space, Header("Match End Special UI")]
    [Space, SerializeField] GameObject _matchEndSpecialUIParent;
    [SerializeField] Button _matchEndSpecialButton;
    [SerializeField] TMP_Text _matchEndSpecialProfitText;

    [Space, Header("Reveal Image")]
    [Space, SerializeField] Image _revealImage;
    [SerializeField, Min(.1f)] float _revealSpeed;
    [SerializeField, Min(0f)] float _revealDelay;

    [Space, Header("Controls")]
    [SerializeField] GameObject _abilityControls;
    [SerializeField] GameObject _playerControls;

    [Space, Header("Other")]
    [SerializeField] UI _defaultScreen;

    public enum UI
    {
        TeamSelection,
        InGame,
        GameEnd,
        GameEndSpecial,
    }

    UI _currentScreen;
    public UI CurrentScreen => _currentScreen;

    Coroutine _revealCoroutine;
    UnityEvent _revealEndBroadcaster;
    #endregion

    new void Awake()
    {
        base.Awake();
        SetScreen(_defaultScreen);
        _revealEndBroadcaster = new UnityEvent();
        _revealEndBroadcaster.AddListener(ShowSpecialEndHiddenElements);
    }

    void Start()
    {
        GameManager.Instance.BalanceChangeBroadcaster.AddListener(UpdateBalanceText);
        GameManager.Instance.MatchTimeBroadcaster.AddListener(UpdateGameTimer);
        GameManager.Instance.PlayerSpawnBroadcaster.AddListener(ShowControls);
        GameManager.Instance.PlayerDespawnBroadcaster.AddListener(ShowAbilitiesList);
        GameManager.Instance.PlayerDespawnBroadcaster.AddListener(HideControls);
        AbilitiesManager.Instance.AddAbilitySelectionBroadCasterListener(HideAbilitiesList);
        AbilitiesManager.Instance.AddAbilitySelectionBroadCasterListener(ShowControls);
        AbilitiesManager.Instance.AddAbilityRemovedBroadcasterListener(ShowAbilitiesList);
        AbilitiesManager.Instance.AddAbilityRemovedBroadcasterListener(HideControls);
        _runnersBetButton.onClick.AddListener(GameManager.Instance.BetOnRunners);
        _catchersBetButton.onClick.AddListener(GameManager.Instance.BetOnCatchers);
        _matchEndSpecialButton.onClick.AddListener(GameManager.StartNewMatch);
        _matchEndButton.onClick.AddListener(GameManager.StartNewMatch);
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
            case UI.GameEnd:
                UpdateEndScreenText();
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
        if (GameManager.Instance.IsSpecialEndMatchActive)
            RevealScreen();
    }

    GameObject GetCurrentScreenParent()
    {
        switch (_currentScreen)
        {
            case UI.TeamSelection:
                return _teamSelectionUIParent;
            case UI.InGame:
                return _inGameUIParent;
            case UI.GameEnd:
                return _matchEndUIParent;
            case UI.GameEndSpecial:
                return _matchEndSpecialUIParent;
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

    void RevealScreen()
    {
        if (_revealCoroutine != null)
            StopCoroutine(_revealCoroutine);
        _revealCoroutine = StartCoroutine(Reveal());

        IEnumerator Reveal()
        {
            _revealImage.gameObject.SetActive(true);
            Color alpha = _revealImage.color;
            alpha.a = 1f;
            _revealImage.color = alpha;

            yield return new WaitForSecondsRealtime(_revealDelay);

            float t = 0;
            while (_revealImage.color.a > 0f)
            {
                float a = Mathf.Lerp(1f, 0f, t);
                if (a < 1f)
                {
                    alpha.a = a;
                    _revealImage.color = alpha;
                }
                t += Time.unscaledDeltaTime * _revealSpeed;
                yield return null;
            }
            _revealImage.gameObject.SetActive(false);
            _revealEndBroadcaster.Invoke();
        }
    }

    #region Hide & Show
    public void ShowSpecialAbility()
    {
        _specialAbility.SetActive(true);
    }

    void ShowControls()
    {
        if (GameManager.Instance.PlayerInstance != null)
        {
            _playerControls.SetActive(true);
            return;
        }
        _abilityControls.SetActive(true);
    }

    void HideControls()
    {
        if (GameManager.Instance.PlayerInstance == null)
            _playerControls.SetActive(false);
        _abilityControls.SetActive(false);
    }
    void HideAbilitiesList()
    {
        _abilitiesParent.gameObject.SetActive(false);
    }
    void ShowAbilitiesList()
    {
        if (GameManager.Instance.PlayerInstance != null)
            return;
        _abilitiesParent.gameObject.SetActive(true);
    }
    
    void ShowSpecialEndHiddenElements()
    {
        if (!_matchEndSpecialUIParent.activeSelf)
            return;
        _matchEndSpecialButton.gameObject.SetActive(true);

        _matchEndSpecialProfitText.gameObject.SetActive(true);
        _matchEndSpecialProfitText.text = GetResultingProfitText();
        _matchEndSpecialProfitText.color = ColorsManager.Instance.Affordable;
    }
    #endregion
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
                runnerDisplayItem.SetSpeedValue(((runnerStats.Speed - runnerStats.SpeedBounds.x) / (runnerStats.SpeedBounds.y - runnerStats.SpeedBounds.x)) * 100f);
                runnerDisplayItem.SetStaminaValue(((runnerStats.Stamina - runnerStats.StaminaBounds.x) / (runnerStats.StaminaBounds.y - runnerStats.StaminaBounds.x)) * 100f);
            }
        }

        void LoadCatchersStatsList()
        {
            foreach (CatcherStats catcherStats in teamsManager.GetCatchersStatsList())
            {
                CatcherStatsDisplay runnerDisplayItem = Instantiate(_catcherStatsDisplayPrefab, _catchersList);
                runnerDisplayItem.SetSpeedValue(((catcherStats.Speed - catcherStats.SpeedBounds.x) / (catcherStats.SpeedBounds.y - catcherStats.SpeedBounds.x)) * 100f);
                runnerDisplayItem.SetCatchRangeValue(((catcherStats.CatchRange - catcherStats.CatchRangeBounds.x) / (catcherStats.CatchRangeBounds.y - catcherStats.CatchRangeBounds.x)) * 100f);
            }
        }
    }
    void LoadAbilityItems()
    {
        foreach (AbilityItem item in AbilitiesManager.Instance.AbilityItems)
            Instantiate(item.gameObject, _abilityItemsListParent);
    }

    #region UI Updates
    void UpdatePlayerTeamText()
    {
        _yourTeamText.color = GameManager.Instance.PlayerTeamColor;
        _yourTeamText.text = GameManager.Instance.PlayerTeam_TAG.ToString();
    }

    void UpdateEndScreenText()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager.MatchWinner == gameManager.PlayerTeam_TEAM_TAG)
            UpdateWin();
        else
            UpdateLoss();
        _resultingProfitText.text = GetResultingProfitText();

        void SetTextsColors(Color color)
        {
            _resultText.color = color;
            _resultingProfitText.color = color;
            _bonusText.color = color;
        }
        void UpdateWin()
        {
            SetTextsColors(ColorsManager.Instance.Affordable);
            _resultText.text = "WIN";
            _bonusText.text = "bonus: +" + gameManager.WinningBonus + "$"; 
        }
        void UpdateLoss()
        {
            SetTextsColors(ColorsManager.Instance.Unaffordable);
            _resultText.text = "LOSS";
            _bonusText.text = "bonus: +" + gameManager.LosingBonus + "$"; 
        }
    }

    String GetResultingProfitText()
    {
        GameManager gameManager = GameManager.Instance;
        if (GameManager.Instance.MatchWinner == GameManager.Instance.PlayerTeam_TEAM_TAG)
        {
            float winScale =  gameManager.PlayerTeam_TEAM_TAG == TagsManager.TeamTag.Runner ? gameManager.RunnersProfitPercentage : gameManager.CatchersProfitPercentage;
            winScale /= 100;
            return "+" + (int)(gameManager.Wager * winScale) + "$";
        }
        else
        {
            float lossScale =  gameManager.PlayerTeam_TEAM_TAG == TagsManager.TeamTag.Runner ? gameManager.RunnersLossPercentage : gameManager.CatchersLossPercentage; 
            lossScale /= 100;
            return (int)(gameManager.Wager * lossScale) + "$";
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

    void UpdateBalanceText()
    {
        int balance = GameManager.Instance.Balance;
        int profit = GameManager.Instance.Profit;
        _balanceText.text = ConvertIntToShortMoney(balance);
        _profitText.color = ColorsManager.GetMoneyColor(profit);
        _profitText.text = "PROFIT: " + ConvertIntToShortMoney(profit);
    }

    void UpdateTeamSelectionUIText()
    {
        GameManager gameManager = GameManager.Instance;
        _teamSelectionUIBalanceText.text = "Balance:\n" + ConvertIntToShortMoney(gameManager.Balance);
        _wagerText.text = "Wager:\n0% 0$";
        _runnersWinText.text = "WIN: +" + gameManager.RunnersProfitPercentage + "%";
        _runnersLossText.text = "LOSS: " + gameManager.RunnersLossPercentage + "%";
        _catchersWinText.text = "WIN: +" + gameManager.CatchersProfitPercentage + "%";
        _catchersLossText.text = "LOSS: " + gameManager.CatchersLossPercentage + "%";
    }
    #endregion
}
