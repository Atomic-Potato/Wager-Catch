﻿using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    #region Global Variables
    [SerializeField] GameState _startingGameState = GameState.InGame;
    
    [Space, Header("MONEY")]
    [SerializeField, Min(0f)] int _minBalance = 150;
    [SerializeField, Min(0f)] int _balance = 150;
    public int Balance => _balance;

    [Space, Header("BETTING")]
    [SerializeField, Min(0)] float MaxProfitMultiplier = 5; 

    [Space]
    [SerializeField] TagsManager.TeamTag _playerTeam = TagsManager.TeamTag.NuteralPlayer;
    public TagsManager.Tag PlayerTeam => TagsManager.ConvertTeamTagToTag(_playerTeam);
    
    [HideInInspector] public Player PlayerInstance;

    public TagsManager.Tag OppositeTeam
    {
        get
        {
            switch (PlayerTeam)
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
    

    float? _matchStartTime;
    float _matchTimePassed;
    public float MatchTimeLeftInSeconds => _matchLengthInSeconds - _matchTimePassed;

    [HideInInspector] public UnityEvent MatchTimeBroadcaster;
    [HideInInspector] public UnityEvent MatchEndBroadcaster;
    [HideInInspector] public CustomUnityEvent BalanceChangeBroadcaster;
    
    GameState _currentGameState;
    public GameState CurrentGameState => _currentGameState;    
    public enum GameState
    {
        InTeamSelection,
        InGame,
    }

    MatchState _currentMatchState = MatchState.Paused;
    public MatchState CurrentMatchState => _currentMatchState;
    public enum MatchState
    {
        Playing,
        Finished,
        Paused
    }

    Winner _matchWinner = Winner.None;
    public Winner MatchWinner => _matchWinner;
    public enum Winner
    {
        None,
        Draw,
        Catchers,
        Runners
    }

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
    #endregion

    new void Awake()
    {
        base.Awake();
        MatchTimeBroadcaster = new UnityEvent();
        MatchEndBroadcaster = new UnityEvent();
        BalanceChangeBroadcaster = new CustomUnityEvent();
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
        if (CurrentGameState == GameState.InGame)
            UpdateGameTimer();
    }

    public void DeductBalance(int amount)
    {
        _balance -= amount;
        if (_balance < 0)
            _balance = 0;
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
        _catchersBetLossScale = teams.RunnersStrengthScale;
        _runnersBetLossScale = teams.CatchersStrengthScale;
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
            _matchWinner = Winner.Runners;
            EndMatch();            
        }
    }

    void UpdateMatchWinnerUsingTeamCount()
    {
        int runnersCount = TeamsManager.Instance.RunnersCount; 
        int catchersCount = TeamsManager.Instance.CatchersCount;

        if (runnersCount == 0 && catchersCount == 0)
            _matchWinner = Winner.Draw;
        else if (catchersCount == 0)
            _matchWinner = Winner.Runners;
        else if (runnersCount == 0)
            _matchWinner = Winner.Catchers;
        
        if (_matchWinner != Winner.None)
                EndMatch();
    }

    void EndMatch()
    {
        if (_currentMatchState == MatchState.Finished)
            return;

        _currentMatchState = MatchState.Finished;
        MatchEndBroadcaster.Invoke();

        Debug.Log("Match winner: " + _matchWinner + "\n" + _currentMatchState);
    }

    void StartMatch()
    {
        Time.timeScale = 1f;
        _currentMatchState = MatchState.Playing;
        if (_matchStartTime == null)
            _matchStartTime = Time.time;
    }

    public void BetOnRunners()
    {
        if (_currentGameState != GameState.InTeamSelection)
            return;
        _playerTeam = TagsManager.TeamTag.Runner;
        _wager = UIManager.Instance.SelectedWager;
        SetGameState(GameState.InGame);
    }

    public void BetOnCatchers()
    {
        if (_currentGameState != GameState.InTeamSelection)
            return;
        _playerTeam = TagsManager.TeamTag.Catcher;
        _wager = UIManager.Instance.SelectedWager;
        SetGameState(GameState.InGame);
    }
}
