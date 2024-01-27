using UnityEngine;

public class NukeEffects : MonoBehaviour
{
    [SerializeField] GameObject _nukeLight;

    void Update()
    {
        SetLightToMousePosition();
        TeamsManager.Instance.GentlemenNowWePanik();
    }

    void OnDisable()
    {
        TeamsManager.Instance.AllIsFineChaps();
    }

    void SetLightToMousePosition()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _nukeLight.transform.position = mousePosition;
    }
}
