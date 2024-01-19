using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeEffects : MonoBehaviour
{
    [SerializeField] GameObject _nukeLight;

    void Update()
    {
        SetLightToMousePosition();
    }

    void SetLightToMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _nukeLight.transform.position = mousePosition;
    }
}
