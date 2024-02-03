using UnityEngine;

public class PitchOscillator : MonoBehaviour
{
    [SerializeField] Vector2 _pitchOscillationRange = new Vector2(.5f, 1.5f);
    [SerializeField, Min(0f)] float _oscillationSpeedMultiplier = 1f; 
    [SerializeField] AudioSource _source;
    
    float t;
    float _tTarge;
    public PitchOscillator(Vector2 pitchRange, float speedMultiplier)
    {
        _pitchOscillationRange = pitchRange;
        _oscillationSpeedMultiplier = speedMultiplier;
    }

    void Awake()
    {
        _tTarge = GetRandomTTarget();
    }

    void Update()
    {
        if (t < _tTarge)
        {
            t += Time.deltaTime * _oscillationSpeedMultiplier;
            if (t >= _tTarge)
                _tTarge = GetRandomTTarget();
        }
        else 
        {
            t -= Time.deltaTime * _oscillationSpeedMultiplier;
            if (t <= _tTarge)
                _tTarge = GetRandomTTarget();
        }
        _source.pitch = Mathf.Lerp(_pitchOscillationRange.x, _pitchOscillationRange.y, t);
    }


    float GetRandomTTarget()
    {
        return Random.value;
    }
}
