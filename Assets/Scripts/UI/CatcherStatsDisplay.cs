using UnityEngine;

public class CatcherStatsDisplay : MonoBehaviour
{
    [SerializeField] RectTransform _catchRangeMeterFill;
    [SerializeField] RectTransform _speedMeterFill;

    public void SetCatchRangeValue(float value)
    {
        if (value < 0 || value > 100)
            return;
        _catchRangeMeterFill.sizeDelta = new Vector2(value, _catchRangeMeterFill.sizeDelta.y);
    }

    public void SetSpeedValue(float value)
    {
        if (value < 0 || value > 100)
            return;
        _speedMeterFill.sizeDelta = new Vector2(value, _speedMeterFill.sizeDelta.y);
    }
}

public class CatcherStats
{
    public float CatchRange;
    public float MaxCatchRange;
    public float Speed;
    public float MaxSpeed;

    public CatcherStats(float catchRange, float maxCatchRange, float speed, float maxSpeed)
    {
        CatchRange = catchRange;
        MaxCatchRange = maxCatchRange;
        Speed = speed;
        MaxSpeed = maxSpeed;
    }    
}