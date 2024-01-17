using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Pathfinding;

public class GameManager : Singleton<GameManager>
{
    #region Global Variables
    [SerializeField] GameState _startingGameState = GameState.InGame;
    
    [Space, Header("MONEY")]
    [SerializeField, Min(0f)] int _minBalance = 150;
    int _balance = 150;
    public int Balance => _balance;
    [Space]
    [SerializeField, Min(0f)] int _winningBonus = 12;
    public int WinningBonus => _winningBonus;
    [SerializeField, Min(0f)] int _losingBonus = 6;
    public int LosingBonus => _losingBonus;
    [Space]
    [SerializeField] int _catchingBonus = 2;
    public int CatchingBonus => _catchingBonus;
    [SerializeField] int _escapingBonus = 1;
    public int EscapingBonus => _escapingBonus;

    [Space, Header("BETTING")]
    [SerializeField, Min(0)] float MaxProfitMultiplier = 5; 

    [Space, Header("COLORS")]
    [SerializeField] Color _runnersColor = new Color(0f, 1f, 0f, 1f);
    public Color RunnersColor => _runnersColor;
    [SerializeField] Color _catchersColor = new Color(1f, 0f, 0f, 1f);
    public Color CatchersColor => _catchersColor;
    [SerializeField] Color _nuteralColor = new Color(1f, 1f, 1f, 1f);
    public Color NuteralColor => _nuteralColor;
    public Color PlayerTeamColor
    {
        get
        {
            switch (PlayerTeam_TAG)
            {
                case TagsManager.Tag.Runner:
                    return RunnersColor;
                case TagsManager.Tag.Catcher:
                    return CatchersColor;
                default:
                    return NuteralColor;
            }
        }
    }

    TagsManager.TeamTag _playerTeam = TagsManager.TeamTag.NuteralPlayer;
    public TagsManager.Tag PlayerTeam_TAG => TagsManager.ConvertTeamTagToTag(_playerTeam);
    public TagsManager.TeamTag PlayerTeam_TEAM_TAG => _playerTeam;
    
    Player _playerInstance;
    [HideInInspector] public Player PlayerInstance => _playerInstance;

    public TagsManager.Tag OppositeTeam
    {
        get
        {
            switch (PlayerTeam_TAG)
            {
                case TagsManager.Tag.Runner:
                    return TagsManager.Tag.Catcher;
                case TagsManager.Tag.Catcher:
                    return TagsManager.Tag.Runner;
                default:
                    return TagsManager.Tag.NuteralPlayer;
            }
        }
    }
    [SerializeField, Min(0f)] float _matchLengthInSeconds = 60f;
    
    [Space]
    [SerializeField] Transform _playerSpawnPoint;
    public Transform PlayerSpawnPoint => _playerSpawnPoint;
    [Tooltip("The point where the player will walk to automatically before gaining control")]
    [SerializeField] Transform _playerEntryPoint;
    public Transform PlayerEntryPoint => _playerEntryPoint;
    
    [Space]
    [SerializeField] LayerMask _explosionLayers;

    float? _matchStartTime;
    float _matchTimePassed;
    public float MatchTimeLeftInSeconds => _matchLengthInSeconds - _matchTimePassed;

    [HideInInspector] public UnityEvent MatchTimeBroadcaster;
    [HideInInspector] public UnityEvent MatchEndBroadcaster;
    [HideInInspector] public UnityEvent PlayerSpawnBroadcaster;
    [HideInInspector] public UnityEvent PlayerDespawnBroadcaster;
    [HideInInspector] public CustomUnityEvent BalanceChangeBroadcaster;
    
    GameState _currentGameState;
    public GameState CurrentGameState => _currentGameState;    
    public enum GameState
    {
        InTeamSelection,
        InGame,
        MatchEnd,
        MatchEndSpecial,
    }

    MatchState _currentMatchState = MatchState.Paused;
    public MatchState CurrentMatchState => _currentMatchState;
    public enum MatchState
    {
        Playing,
        Finished,
        Paused
    }

    TagsManager.TeamTag _matchWinner = TagsManager.TeamTag.None;
    public TagsManager.TeamTag MatchWinner => _matchWinner;

    int _wager;
    public int Wager => _wager;

    float _runnersBetProfitScale;
    public int RunnersProfitPercentage => (int)(_runnersBetProfitScale * 100f);
    float _runnersBetLossScale;
    public int RunnersLossPercentage => (int)(_runnersBetLossScale * 100f);
    float _catchersBetProfitScale;
    public int CatchersProfitPercentage => (int)(_catchersBetProfitScale * 100f);
    float _catchersBetLossScale;
    public int CatchersLossPercentage => (int)(_catchersBetLossScale * 100f);

    int _profit;
    public int Profit => _profit;

    bool _isSpecialEndMatchActive;
    public bool IsSpecialEndMatchActive => _isSpecialEndMatchActive;
    #endregion

    #region Execution
    new void Awake()
    {
        base.Awake();
        MatchTimeBroadcaster = new UnityEvent();
        MatchEndBroadcaster = new UnityEvent();
        PlayerSpawnBroadcaster = new UnityEvent();
        PlayerDespawnBroadcaster = new UnityEvent();
        BalanceChangeBroadcaster = new CustomUnityEvent();
        PlayerData data = DataSavingManager.LoadData();
        _balance = data.Balance < _minBalance ? _minBalance : data.Balance;
        
    }

    void Start()
    {
        TeamsManager.Instance.TeamsCountBroadcaster.AddListener(UpdateMatchWinnerUsingTeamCount);
        CalculateTeamsBetProfitScale();
        CalculateTeamsBetLossScale();
        SetGameState(_startingGameState);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene("SampleScene");
        if (Input.GetKeyDown(KeyCode.E))
            ImpulseObjectsInPorximity(Camera.main.ScreenToWorldPoint(Input.mousePosition), _explosionLayers);
#endif

        if (CurrentGameState == GameState.InGame)
            UpdateGameTimer();
    }
    #endregion

    public void ActivateSpecialEnd()
    {
        _isSpecialEndMatchActive = true;
    }

    public static void StartNewMatch()
    {
        DataSavingManager.Save();
        SceneManager.LoadScene("SampleScene");
    }

    public void SetPlayerInstance(Player player)
    {
        _playerInstance  = player;
        PlayerSpawnBroadcaster.Invoke();
    }

    public void RemovePlayerInstance()
    {
        _playerInstance  = null;
        PlayerDespawnBroadcaster.Invoke();
    }

    public void DeductBalance(int amount)
    {
        _balance -= amount;
        _profit -= amount;
        if (_balance < 0)
            _balance = 0;
        BalanceChangeBroadcaster.Invoke();
    }

    public void AddBalance(int amount)
    {
        _balance += amount;
        BalanceChangeBroadcaster.Invoke();
    }

    void SetGameState(GameState state)
    {
        _currentGameState = state;
        switch (state)
        {
            case GameState.InTeamSelection:
                Time.timeScale = 0f;
                UIManager.Instance.SetScreen(UIManager.UI.TeamSelection);
                break;
            case GameState.InGame:
                StartMatch();
                UIManager.Instance.SetScreen(UIManager.UI.InGame);
                break;
            case GameState.MatchEnd:
                Time.timeScale = 0f;
                UIManager.Instance.SetScreen(UIManager.UI.GameEnd);
                break;
            case GameState.MatchEndSpecial:
                Time.timeScale = 0f;
                UIManager.Instance.SetScreen(UIManager.UI.GameEndSpecial);
                break;
        }
    }

    void CalculateTeamsBetProfitScale()
    {
        TeamsManager teams = TeamsManager.Instance;
        _catchersBetProfitScale = teams.RunnersStrengthScale * MaxProfitMultiplier;
        _runnersBetProfitScale = teams.CatchersStrengthScale * MaxProfitMultiplier;
    }
    
    void CalculateTeamsBetLossScale()
    {
        TeamsManager teams = TeamsManager.Instance;
        _catchersBetLossScale = -teams.RunnersStrengthScale;
        _runnersBetLossScale = -teams.CatchersStrengthScale;
    }

    void UpdateGameTimer()
    {
        if (_currentMatchState == MatchState.Finished || _matchStartTime == null)
            return;

        if (_matchTimePassed < _matchLengthInSeconds)
        {
            _matchTimePassed = Time.time - (float)_matchStartTime;
            MatchTimeBroadcaster.Invoke();
        }
        else
        {
            _matchTimePassed = _matchLengthInSeconds;
            _matchWinner = TagsManager.TeamTag.Runner;
            EndMatch();            
        }
    }

    void UpdateMatchWinnerUsingTeamCount()
    {
        if (_isSpecialEndMatchActive)
            _matchWinner = _playerTeam;
        else
        {
            int runnersCount = TeamsManager.Instance.RunnersCount; 
            int catchersCount = TeamsManager.Instance.CatchersCount;

            if (runnersCount == 0 && catchersCount == 0)
                _matchWinner = TagsManager.TeamTag.NuteralPlayer;
            else if (catchersCount == 0)
                _matchWinner = TagsManager.TeamTag.Runner;
            else if (runnersCount == 0)
                _matchWinner = TagsManager.TeamTag.Catcher;
        }

        
        if (_matchWinner != TagsManager.TeamTag.None)
                EndMatch();
    }

    void EndMatch()
    {
        if (_currentMatchState == MatchState.Finished)
            return;

        _currentMatchState = MatchState.Finished;
        MatchEndBroadcaster.Invoke();
        if (_matchWinner == _playerTeam)
            AddWinningsToBalance();
        else
            AddRemainingWagedMoney();

        GameState endState = _isSpecialEndMatchActive ? GameState.MatchEndSpecial : GameState.MatchEnd;         
        SetGameState(endState);

        void AddWinningsToBalance()
        {
            int winnings = _wager;
            if (_matchWinner == TagsManager.TeamTag.Runner)
                winnings = (int)(winnings + winnings * _runnersBetProfitScale);
            else if(_matchWinner == TagsManager.TeamTag.Catcher)
                winnings = (int)(winnings + winnings * _catchersBetProfitScale);
            winnings += _winningBonus;
            _balance += winnings;
        }
        void AddRemainingWagedMoney()
        {
            int remaining = _wager;
            if (_matchWinner == TagsManager.TeamTag.Runner)
                remaining = (int)(remaining * _catchersBetLossScale);
            else if(_matchWinner == TagsManager.TeamTag.Catcher)
                remaining = (int)(remaining * _runnersBetLossScale);
            remaining += _losingBonus;
            _balance += remaining;
        }
    }

    void StartMatch()
    {
        _currentMatchState = MatchState.Playing;
        Time.timeScale = 1f;

        _wager = UIManager.Instance.SelectedWager;
        _balance -= _wager;

        
        _profit = (int)(_wager * GetProfitScale());

        if (_matchStartTime == null)
            _matchStartTime = Time.time;

        float GetProfitScale()
        {
            switch (_playerTeam)
            {
                case TagsManager.TeamTag.Runner:
                    return _runnersBetProfitScale;
                case TagsManager.TeamTag.Catcher:
                    return _catchersBetProfitScale;
                default:
                    return 0f;
            }
        }
    }

    public void BetOnRunners()
    {
        if (_currentGameState != GameState.InTeamSelection)
            return;
        _playerTeam = TagsManager.TeamTag.Runner;
        SetGameState(GameState.InGame);
    }

    public void BetOnCatchers()
    {
        if (_currentGameState != GameState.InTeamSelection)
            return;
        _playerTeam = TagsManager.TeamTag.Catcher;
        SetGameState(GameState.InGame);
    }

    #region Editor Functions
    public static void ImpulseObjectsInPorximity(Vector2 position, LayerMask affectedLayers)
    {
        RaycastHit2D[] hits =  Physics2D.CircleCastAll(position, 2, Vector2.zero, 50f, affectedLayers);
        foreach (RaycastHit2D hit in hits)
        {
            TeamPlayer teamPlayer = hit.collider.gameObject.GetComponent<TeamPlayer>();
            if (teamPlayer != null)
            {
                if (teamPlayer is Runner && ((Runner)teamPlayer).IsInSafeArea)
                    continue;
                teamPlayer.ImpulseFromPoint(position);
            }
            else
            {
                UnitBase unit = hit.collider.gameObject.GetComponent<UnitBase>();
                unit.ImpulseFromPoint(position);
            }
        }
    }
    #endregion
}
