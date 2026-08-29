using UnityEngine;

/// <summary>
/// Cycles a wheel SpriteRenderer through a set of pre-shifted "tread scroll" frames
/// to fake rolling motion on a top-down sprite (rotating the whole wheel sprite
/// looks wrong from directly above, since the visible chunk isn't a full circle -
/// so instead we scroll the tread pattern inside a fixed silhouette).
///
/// Assign the 8 frames from wheel_<name>_f0..f7.png (or slice the *_rollsheet.png)
/// in order into the `frames` array.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class WheelRoll : MonoBehaviour
{
    [Tooltip("Ordered animation frames (0..7), same order as the exported files.")]
    public Sprite[] frames;

    [Tooltip("How many frames to advance per 1 unit of forward travel. Tune to taste.")]
    public float framesPerUnitDistance = 4f;

    [Tooltip("Reference to the vehicle root, used to measure how far it has moved. " +
             "Leave empty to use this object's own parent.")]
    public Transform vehicleRoot;

    SpriteRenderer sr;
    Vector3 lastPos;
    float frameAccumulator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (vehicleRoot == null && transform.parent != null)
            vehicleRoot = transform.parent;
        if (vehicleRoot != null)
            lastPos = vehicleRoot.position;
    }

    void Update()
    {
        if (frames == null || frames.Length == 0 || vehicleRoot == null) return;

        float traveled = Vector3.Distance(vehicleRoot.position, lastPos);

        // Sign of travel: positive if moving in the vehicle's forward direction,
        // negative if reversing, so the tread scrolls the correct way.
        Vector3 delta = vehicleRoot.position - lastPos;
        float dir = Mathf.Sign(Vector3.Dot(delta, vehicleRoot.up));
        lastPos = vehicleRoot.position;

        frameAccumulator += traveled * framesPerUnitDistance * dir;

        int index = Mathf.RoundToInt(frameAccumulator) % frames.Length;
        if (index < 0) index += frames.Length;

        sr.sprite = frames[index];
    }

    /// <summary>Call this instead if you'd rather drive frames directly from a speed value.</summary>
    public void AdvanceBySpeed(float speed, float deltaTime)
    {
        if (frames == null || frames.Length == 0) return;
        frameAccumulator += speed * framesPerUnitDistance * deltaTime;
        int index = Mathf.RoundToInt(frameAccumulator) % frames.Length;
        if (index < 0) index += frames.Length;
        sr.sprite = frames[index];
    }
}
