using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    #region Global Variables
    [SerializeField] GameState _startingGameState = GameState.InGame;
    
    [Space]
    [SerializeField, Min(0f)] int _minBalance = 150;
    [SerializeField, Min(0f)] int _balance = 150;
    public int Balance => _balance;

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
    [HideInInspector] public GameState CurrentGameState;    
    public enum GameState
    {
        InMenu,
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
    #endregion

    new void Awake()
    {
        base.Awake();
        CurrentGameState = _startingGameState;
        MatchTimeBroadcaster = new UnityEvent();
        MatchEndBroadcaster = new UnityEvent();
        BalanceChangeBroadcaster = new CustomUnityEvent();
    }

    void Start()
    {
        TeamsManager.Instance.TeamsCountBroadcaster.AddListener(UpdateMatchWinnerUsingTeamCount);
        StartMatch();
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
        _currentMatchState = MatchState.Playing;
        if (_matchStartTime == null)
            _matchStartTime = Time.time;
    }
}
