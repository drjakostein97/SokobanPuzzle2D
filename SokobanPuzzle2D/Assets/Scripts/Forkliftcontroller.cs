using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down, car-style controller for the forklift. Put this on the root
/// "Forklift" GameObject (the same Transform that WheelRoll's `vehicleRoot`
/// points at). Requires a Rigidbody2D on the same object, Body Type = Dynamic,
/// Gravity Scale = 0 (this is a top-down game, not side-on).
///
/// Uses the new Input System package directly (Keyboard.current) so it works
/// out of the box with no Input Actions asset needed. Requires the
/// "Input System" package to be installed and Player Settings > Active Input
/// Handling set to "Input System Package" or "Both".
///
/// Wires straight into the wheel split/animation set from before:
///  - feeds current speed to every WheelRoll each frame so tread scroll matches
///    actual motion (including correctly reversing when backing up)
///  - feeds steering input to the front WheelSteer components so the front
///    wheels visually turn
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ForkliftController : MonoBehaviour
{
    [Header("Driving")]
    [Tooltip("Top forward speed, units/sec.")]
    public float maxSpeed = 6f;
    [Tooltip("Top reverse speed, units/sec (kept lower than forward, like a real forklift).")]
    public float maxReverseSpeed = 3f;
    [Tooltip("How fast the forklift accelerates toward its target speed.")]
    public float acceleration = 12f;
    [Tooltip("How fast it slows down when no input is held.")]
    public float deceleration = 10f;
    [Tooltip("How fast it can brake when input reverses direction.")]
    public float brakeDeceleration = 20f;

    [Header("Turning")]
    [Tooltip("Max turn rate in degrees/sec, reached at full speed.")]
    public float maxTurnRate = 110f;
    [Tooltip("Turning is scaled by current speed so it doesn't spin in place at a standstill. " +
             "This is the fraction of maxTurnRate available even at zero speed (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float minTurnFactor = 0.15f;

    [Header("Wheel visuals (optional, drag in from the child wheel objects)")]
    public WheelRoll[] wheelRollers;
    public WheelSteer[] frontWheelSteerers;

    Rigidbody2D rb;
    float currentSpeed;      // signed: positive = forward, negative = reverse
    float steerInput;        // -1..1, cached for wheel visuals

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true; // we rotate manually for predictable turning
    }

    void Update()
    {
        float throttleInput = ReadThrottleInput(); // W/S or Up/Down, -1..1
        steerInput = ReadSteerInput();              // A/D or Left/Right, -1..1

        UpdateSpeed(throttleInput, Time.deltaTime);
        UpdateWheelVisuals(Time.deltaTime);
    }

    static float ReadThrottleInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return 0f;
        float value = 0f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) value += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) value -= 1f;
        return value;
    }

    static float ReadSteerInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return 0f;
        float value = 0f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) value += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) value -= 1f;
        return value;
    }

    void FixedUpdate()
    {
        // Move forward along the vehicle's "up" (matches the sprite facing up in its texture).
        Vector2 forward = transform.up;
        rb.MovePosition(rb.position + forward * currentSpeed * Time.fixedDeltaTime);

        // Turn rate scales with how fast we're going, and reverses sign when reversing
        // (so steering left while backing up swings the rear the way you'd expect).
        float speedFrac = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        float turnFactor = Mathf.Lerp(minTurnFactor, 1f, speedFrac);
        float directionSign = currentSpeed >= 0f ? 1f : -1f;

        float turn = -steerInput * maxTurnRate * turnFactor * directionSign * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + turn);
    }

    void UpdateSpeed(float throttleInput, float dt)
    {
        float target = throttleInput >= 0f
            ? throttleInput * maxSpeed
            : throttleInput * maxReverseSpeed;

        bool reversingDirection = Mathf.Sign(target) != 0f &&
                                   Mathf.Sign(currentSpeed) != 0f &&
                                   Mathf.Sign(target) != Mathf.Sign(currentSpeed);

        float rate;
        if (Mathf.Abs(target) < 0.01f)
            rate = deceleration;
        else if (reversingDirection)
            rate = brakeDeceleration;
        else
            rate = acceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, target, rate * dt);
    }

    void UpdateWheelVisuals(float dt)
    {
        if (wheelRollers != null)
            foreach (var w in wheelRollers)
                if (w != null) w.AdvanceBySpeed(currentSpeed, dt);

        if (frontWheelSteerers != null)
            foreach (var s in frontWheelSteerers)
                if (s != null) s.SetSteerInput(-steerInput);
    }

    /// <summary>Current signed speed, forward-positive. Handy for UI, sound, etc.</summary>
    public float CurrentSpeed => currentSpeed;
}
