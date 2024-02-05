using UnityEngine;
using UnityEngine.Events;

public class RemoteAnimationEvent : MonoBehaviour
{
    public UnityEvent AnimationEvent;

    public void InvokeEvent()
    {
        AnimationEvent.Invoke();
    }
}
