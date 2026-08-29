using UnityEngine;

/// <summary>
/// Rotates a front wheel sprite for visual steering feedback (unlike rolling,
/// in-plane rotation is correct for steering since the wheel is turning around
/// its vertical axis, not its axle).
/// Put this on each front wheel child; leave rear wheels alone.
/// </summary>
public class WheelSteer : MonoBehaviour
{
    [Tooltip("Max steering angle in degrees, either direction.")]
    public float maxSteerAngle = 30f;

    [Tooltip("How quickly the wheel sprite catches up to the target angle.")]
    public float turnSpeed = 8f;

    float targetAngle;

    /// <summary> steerInput: -1 (full left) .. 1 (full right) </summary>
    public void SetSteerInput(float steerInput)
    {
        targetAngle = Mathf.Clamp(steerInput, -1f, 1f) * maxSteerAngle;
    }

    void Update()
    {
        float current = transform.localEulerAngles.z;
        if (current > 180f) current -= 360f;
        float next = Mathf.LerpAngle(current, targetAngle, Time.deltaTime * turnSpeed);
        transform.localRotation = Quaternion.Euler(0f, 0f, next);
    }
}
