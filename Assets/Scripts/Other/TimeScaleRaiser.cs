using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimeScaleRaiser : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Min(0f)] float _timeScaleMultiplier = 2f;
    [SerializeField] Button _button;

    float _originalTimeScale;
    


    void RestoreTime()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButton(1))
            return;
        SpeedUpTime();
        void SpeedUpTime()
        {
            _originalTimeScale = Time.timeScale;
            Time.timeScale *= _timeScaleMultiplier;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(1))
            return;
        Time.timeScale = _originalTimeScale;
    }
}
