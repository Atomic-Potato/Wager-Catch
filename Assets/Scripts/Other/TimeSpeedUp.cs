using UnityEngine;
using UnityEngine.EventSystems;

public class TimeSpeedUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Min(0f)] float _timeScaleMultiplier = 3.5f;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetMouseButton(1))
            return;
        TimeScaleManipulator.Instance.ChangeScale(_timeScaleMultiplier);
        SoundManager.Instance.PlaySoundAtPosition(Vector2.zero, SoundManager.Sound.Click_1);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Input.GetMouseButtonUp(1))
            return;
        // TimeScaleManipulator.Instance.RestoreTime();
        SoundManager.Instance.PlaySoundAtPosition(Vector2.zero, SoundManager.Sound.Click_2);
    }
}
