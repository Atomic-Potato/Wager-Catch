using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomUnityEvent : UnityEvent
{
    public bool IsActive = true;
    List<UnityAction> _listeners = new List<UnityAction>();

    public new void AddListener(UnityAction call)
    {
        base.AddListener(call);
        _listeners.Add(call);
    }

    public new void RemoveListener(UnityAction call)
    {
        base.RemoveListener(call);
        _listeners.Remove(call);
    }

    public new bool Invoke()
    {
        if (IsActive)
        {
            base.Invoke();
            return true;
        }
        return false;
    }
}