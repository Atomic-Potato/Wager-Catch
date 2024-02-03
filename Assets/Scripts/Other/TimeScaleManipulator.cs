using UnityEngine;
using UnityEngine.Events;

public class TimeScaleManipulator : Singleton<TimeScaleManipulator>
{
    [HideInInspector] public UnityEvent TimeScaleIncreasedBrodcaster;
    [HideInInspector] public UnityEvent TimeScaleDecreasedBrodcaster;
    [HideInInspector] public UnityEvent TimeScaleRestoredBroadcaster;
    
    new void Awake()
    {
        base.Awake();

        TimeScaleDecreasedBrodcaster = new UnityEvent();
        TimeScaleIncreasedBrodcaster = new UnityEvent();
        TimeScaleRestoredBroadcaster = new UnityEvent();
    }

    public void SpeedUpTime(float scaleMultiplier)
    {
        Time.timeScale *= scaleMultiplier;
        if (scaleMultiplier > 1f)
            TimeScaleIncreasedBrodcaster.Invoke();
        else
            TimeScaleDecreasedBrodcaster.Invoke();
    }

    public void RestoreTime()
    {
        Time.timeScale = 1f;
        TimeScaleRestoredBroadcaster.Invoke();
    }
}
