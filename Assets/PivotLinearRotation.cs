using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotLinearRotation : MonoBehaviour
{
    [SerializeField, Min(0f)] float speedRange = 1f;
    [SerializeField, Min(0f)] float speedSwitchFactor = .15f;
    float speed;
    float t;
    bool is1;

    void Update()
    {
        if (!is1)
        {
            t += Time.deltaTime * speedSwitchFactor;
            if (t > 1f)
            {
                is1 = true;
                t = 1f;
            } 
        }
        else
        {
            t -= Time.deltaTime * speedSwitchFactor;
            if (t < 0f)
            {
                is1 = false;
                t = 0f;
            } 
        }
        
        speed = Mathf.Lerp(-speedRange, speedRange, t);
        Debug.Log(speed);

        Vector3 currentEulerAngles = transform.rotation.eulerAngles;
        float newRotation = currentEulerAngles.z + Time.deltaTime * speed;
        transform.rotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, newRotation);
    }
}
