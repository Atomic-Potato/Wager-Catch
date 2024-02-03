using UnityEngine;
using UnityEngine.EventSystems;

public class TimeSpeedUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Min(1f)] float _timeScaleMultiplier = 3.5f;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButton(1))
            return;
        TimeScaleManipulator.Instance.SpeedUpTime(_timeScaleMultiplier);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(1))
            return;
        TimeScaleManipulator.Instance.RestoreTime();
    }
}
