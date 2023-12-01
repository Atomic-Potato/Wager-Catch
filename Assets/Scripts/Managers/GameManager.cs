using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GameState _startingGameState = GameState.InGame;
    [SerializeField, Min(0f)] float _matchLengthInSeconds = 60f;
    
    float? _matchStartTime;
    float _matchTimePassed;
    public float MatchTimeLeftInSeconds => _matchLengthInSeconds - _matchTimePassed;

    [HideInInspector] public UnityEvent MatchTimeBroadcaster;
    [HideInInspector] public UnityEvent MatchEndBroadcaster;
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

    new void Awake()
    {
        base.Awake();
        CurrentGameState = _startingGameState;
        MatchTimeBroadcaster = new UnityEvent();
        MatchEndBroadcaster = new UnityEvent();
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

        Debug.Log("R: " + runnersCount + " C : " + catchersCount);        

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
