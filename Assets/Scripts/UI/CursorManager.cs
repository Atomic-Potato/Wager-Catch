using System;
using System.Collections;
using System.Collections.Generic;
using Ability;
using UnityEngine;

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField] Texture2D _pointing;
    [SerializeField] Texture2D _grab;
    [SerializeField] Texture2D _grabbing;
    [SerializeField] Texture2D _crosshair;

    CursorType _currentCursor  = CursorType.None;
    enum CursorType
    {
        None, Pointing, Grab, Grabbing, Crosshair,
    }

    new void Awake()
    {
        base.Awake();
        SetCursor(_pointing, new Vector2(_pointing.width/2f, 0f), CursorType.Pointing);
    }

    void Start()
    {
        GameManager.Instance.PlayerSpawnBroadcaster.AddListener(HideCursor);
        GameManager.Instance.PlayerDespawnBroadcaster.AddListener(ShowCursor);
    }

    void Update()
    {
        if (AbilitiesManager.Instance.IsAbilitySelected)
            SetCursor(_grabbing, new Vector2(_grabbing.width/2f, _grabbing.height/2f), CursorType.Grabbing);
        else
            SetCursor(_pointing, new Vector2(_pointing.width/2f, 0f), CursorType.Pointing);
    }

    void SetCursor(Texture2D texture, Vector2 hotSpot, CursorType cursor)
    {
        if (_currentCursor == cursor)
            return;

        Cursor.SetCursor(texture, hotSpot, CursorMode.ForceSoftware);
        _currentCursor = cursor;
    }

    void HideCursor()
    {
        Cursor.visible = false;
    }

    void ShowCursor()
    {
        Cursor.visible = true;
    }
}
