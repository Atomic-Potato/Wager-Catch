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
    public Vector2 CatchRangeBounds;
    public float Speed;
    public Vector2 SpeedBounds;

    public CatcherStats(float catchRange, Vector2 catchRangeBounds, float speed, Vector2 speedRange)
    {
        CatchRange = catchRange;
        CatchRangeBounds = catchRangeBounds;
        Speed = speed;
        SpeedBounds = speedRange;
    }    
}