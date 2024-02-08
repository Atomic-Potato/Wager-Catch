using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MainMenu : UIMenuBase
{
    [Tooltip("Should cover the entire menu. It diables palyer interaction with the menu when enabled")]
    [SerializeField] RawImage _invisibleImage;

    [Space, Header("Others")]
    [SerializeField] UIMenuBase _mapSelectionMenu;

    public override void ShowMenu()
    {
        base.ShowMenu();
        _invisibleImage.enabled = false;
    }

    #region Buttons Logic
    public void ExecuteStart()
    {
        _mapSelectionMenu.ShowMenu();
        _invisibleImage.enabled = true;
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
