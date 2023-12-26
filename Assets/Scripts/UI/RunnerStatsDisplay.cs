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
    public float MaxStamina;
    public float Speed;
    public float MaxSpeed;

    public RunnerStats(float stamina, float maxStamina, float speed, float maxSpeed)
    {
        Stamina = stamina;
        MaxStamina = maxStamina;
        Speed = speed;
        MaxSpeed = maxSpeed;
    }    
}