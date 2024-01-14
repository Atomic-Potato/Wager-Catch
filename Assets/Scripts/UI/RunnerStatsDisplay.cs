using UnityEngine;

public class RunnerStatsDisplay : MonoBehaviour
{
    [SerializeField] RectTransform _staminaMeterFill;
    [SerializeField] RectTransform _speedMeterFill;

    public void SetStaminaValue(float value)
    {
        if (value < 0 || value > 100)
            return;
        _staminaMeterFill.sizeDelta = new Vector2(value, _staminaMeterFill.sizeDelta.y);
    }

    public void SetSpeedValue(float value)
    {
        if (value < 0 || value > 100)
            return;
        _speedMeterFill.sizeDelta = new Vector2(value, _speedMeterFill.sizeDelta.y);
    }
}

public class RunnerStats
{
    public float Stamina;
    public Vector2 StaminaBounds;
    public float Speed;
    public Vector2 SpeedBounds;

    public RunnerStats(float stamina, Vector2 staminaBounds, float speed, Vector2 speedBounds)
    {
        Stamina = stamina;
        StaminaBounds = staminaBounds;
        Speed = speed;
        SpeedBounds = speedBounds;
    }    
}