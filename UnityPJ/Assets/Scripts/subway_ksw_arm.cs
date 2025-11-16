using UnityEngine;

public class subway_ksw_arm : MonoBehaviour
{
    Quaternion startRotation;
    public float swingAngle = 10.0f;
    public float swingSpeed = 2.0f;

    void Start()
    {
        startRotation= transform.localRotation; 
    }
    void Update()
    {
            float sinValue = Mathf.Sin(Time.time * swingSpeed);
            float currentAngle = sinValue * swingAngle;
            transform.localRotation = startRotation * Quaternion.Euler(0, currentAngle, 0);
    }
}
