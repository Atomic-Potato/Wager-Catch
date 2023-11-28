using TMPro;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text _gameTimerText;

    void Start()
    {
        GameManager.Instance.MatchTimeBroadcaster.AddListener(UpdateGameTimer);
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
}
