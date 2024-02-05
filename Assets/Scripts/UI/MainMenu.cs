using System;
using UnityEngine;

[Serializable]
public class MainMenu : UIMenuBase
{
    #region Buttons Logic
    public void ExecuteStart()
    {
        Debug.Log("Start pressed");
    }

    public void ExecuteChaosMode()
    {
        Debug.Log("Chaos Mode pressed");

    }

    public void ExecuteCredits()
    {
        Debug.Log("Credits pressed");

    }

    public void ExecuteExit()
    {
        Debug.Log("Exit pressed");

    }
    #endregion
}
