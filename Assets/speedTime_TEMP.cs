using System.Collections;
using UnityEngine;

public class speedTime_TEMP : MonoBehaviour
{
    [SerializeField] bool active;
    [SerializeField] float scale;
    [SerializeField] float delay = 1f;
    void Awake()
    {
        Time.timeScale = active ? scale : 1f;
        StartCoroutine(PlaySound());
    }

    void Update()
    {
        Time.timeScale = active ? scale : 1f;
    }

    IEnumerator PlaySound()
    {
        while(true)
        {
            SoundManager.Instance.PlaySoundAtPosition(Vector2.zero, SoundManager.Sound.Death);
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}
