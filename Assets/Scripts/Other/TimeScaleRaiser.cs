using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimeScaleRaiser : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Min(0f)] float _timeScaleMultiplier = 2f;
    [SerializeField] Button _button;

    float _originalTimeScale;
    

    void FastForwardTime()
    {
        
    }

    void RestoreTime()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _originalTimeScale = Time.timeScale;
        Time.timeScale *= _timeScaleMultiplier;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Time.timeScale = _originalTimeScale;
    }
}
