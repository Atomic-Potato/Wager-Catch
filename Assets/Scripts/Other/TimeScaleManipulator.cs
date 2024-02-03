using UnityEngine;
using UnityEngine.Events;

public class TimeScaleManipulator : Singleton<TimeScaleManipulator>
{
    public UnityEvent TimeScaleIncreasedBrodcaster;
    public UnityEvent TimeScaleDecreasedBrodcaster;
    public UnityEvent TimeScaleRestoredBroadcaster;
    public UnityEvent TimeScalePausedBroadcaster;
    
    new void Awake()
    {
        base.Awake();

        TimeScaleDecreasedBrodcaster = new UnityEvent();
        TimeScaleIncreasedBrodcaster = new UnityEvent();
        TimeScaleRestoredBroadcaster = new UnityEvent();
        TimeScalePausedBroadcaster = new UnityEvent();
    }

    public void ChangeScale(float scaleMultiplier)
    {
        if (scaleMultiplier < 0f)
            throw new System.Exception("Cannot set negative time scale");

        Time.timeScale *= scaleMultiplier;
        if (scaleMultiplier > 1f)
            TimeScaleIncreasedBrodcaster.Invoke();
        else
            TimeScaleDecreasedBrodcaster.Invoke();
    }

    public void PauseTime()
    {
        Time.timeScale = 0f;
        TimeScalePausedBroadcaster.Invoke();
    }

    public void RestoreTime()
    {
        Time.timeScale = 1f;
        TimeScaleRestoredBroadcaster.Invoke();
    }
}
