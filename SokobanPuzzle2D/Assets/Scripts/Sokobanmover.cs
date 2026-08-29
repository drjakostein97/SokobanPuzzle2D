using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sokoban-style grid movement: the forklift moves exactly one tile per input,
/// snapped to a grid, and pushes a box in front of it if the tile beyond the
/// box is free. Replaces ForkliftController for puzzle-style movement.
///
/// Put this on the root "Forklift" GameObject (same one WheelRoll's
/// `vehicleRoot` points at). No Rigidbody2D needed — this moves the transform
/// directly and uses OverlapBox checks for walls/boxes, so set that up via
/// layers (see Wall Mask / Box Mask below).
/// </summary>
public class SokobanMover : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("World-space size of one tile. Must match your level's grid spacing.")]
    public float cellSize = 1f;
    [Tooltip("Seconds to glide from one tile to the next.")]
    public float moveDuration = 0.15f;

    [Header("Collision")]
    [Tooltip("Half-extent of the overlap check box, in world units. Slightly smaller than cellSize/2 avoids false positives with neighboring tiles.")]
    public float checkBoxHalfExtent = 0.4f;
    [Tooltip("Layer(s) that block movement outright (walls).")]
    public LayerMask wallMask;
    [Tooltip("Layer(s) for pushable boxes.")]
    public LayerMask boxMask;

    [Header("Wheel visuals (optional)")]
    public WheelRoll[] wheelRollers;

    [Header("Facing")]
    [Tooltip("Instantly snap to face the move direction. Turn off for a smooth rotate instead.")]
    public bool snapFacing = true;
    [Tooltip("Degrees/sec when snapFacing is off.")]
    public float faceTurnSpeed = 720f;

    bool isMoving;
    Vector2 pendingFaceDir;
    bool hasPendingFace;

    void Awake()
    {
        // Snap to the grid at startup so pushes line up cleanly.
        Vector3 p = transform.position;
        p.x = Mathf.Round(p.x / cellSize) * cellSize;
        p.y = Mathf.Round(p.y / cellSize) * cellSize;
        transform.position = p;
    }

    void Update()
    {
        if (isMoving) return;

        Vector2Int dir = ReadDirectionInput();
        if (dir == Vector2Int.zero) return;

        TryMove(dir);
    }

    static Vector2Int ReadDirectionInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector2Int.zero;

        // Prioritize whichever was pressed this frame; simple single-axis read,
        // fine for turn-based/grid puzzle input (no diagonals).
        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame) return Vector2Int.up;
        if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame) return Vector2Int.down;
        if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) return Vector2Int.left;
        if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) return Vector2Int.right;

        return Vector2Int.zero;
    }

    void TryMove(Vector2Int dir)
    {
        Vector2 worldDir = new Vector2(dir.x, dir.y);
        Vector3 targetPos = transform.position + (Vector3)(worldDir * cellSize);

        // Always face the direction you tried to move, even if the move is blocked
        // (matches classic Sokoban feel: bumping a wall still turns you to face it).
        FaceDirection(worldDir);

        Collider2D boxAhead = Physics2D.OverlapBox(targetPos, Vector2.one * checkBoxHalfExtent * 2f, 0f, boxMask);
        Collider2D wallAhead = Physics2D.OverlapBox(targetPos, Vector2.one * checkBoxHalfExtent * 2f, 0f, wallMask);

        if (wallAhead != null) return; // blocked, can't move at all

        if (boxAhead != null)
        {
            Vector3 boxTargetPos = targetPos + (Vector3)(worldDir * cellSize);
            Collider2D behindBox = Physics2D.OverlapBox(boxTargetPos, Vector2.one * checkBoxHalfExtent * 2f, 0f,
                wallMask | boxMask);
            if (behindBox != null) return; // box can't be pushed, so the forklift can't move either

            StartCoroutine(MoveBoth(boxAhead.transform, boxTargetPos, targetPos));
        }
        else
        {
            StartCoroutine(MoveSelf(targetPos));
        }
    }

    void FaceDirection(Vector2 worldDir)
    {
        float targetAngle = Mathf.Atan2(worldDir.y, worldDir.x) * Mathf.Rad2Deg - 90f; // sprite faces up
        if (snapFacing)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            pendingFaceDir = worldDir;
            hasPendingFace = true;
        }
    }

    IEnumerator MoveSelf(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / moveDuration);
            transform.position = Vector3.Lerp(start, targetPos, frac);
            RollWheels(frac);
            RotateTowardFacing();
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }

    IEnumerator MoveBoth(Transform box, Vector3 boxTargetPos, Vector3 selfTargetPos)
    {
        isMoving = true;
        Vector3 selfStart = transform.position;
        Vector3 boxStart = box.position;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / moveDuration);
            transform.position = Vector3.Lerp(selfStart, selfTargetPos, frac);
            box.position = Vector3.Lerp(boxStart, boxTargetPos, frac);
            RollWheels(frac);
            RotateTowardFacing();
            yield return null;
        }
        transform.position = selfTargetPos;
        box.position = boxTargetPos;
        isMoving = false;
    }

    void RollWheels(float frac)
    {
        if (wheelRollers == null) return;
        float speed = cellSize / moveDuration; // constant during the glide
        foreach (var w in wheelRollers)
            if (w != null) w.AdvanceBySpeed(speed, Time.deltaTime);
    }

    void RotateTowardFacing()
    {
        if (snapFacing || !hasPendingFace) return;
        float targetAngle = Mathf.Atan2(pendingFaceDir.y, pendingFaceDir.x) * Mathf.Rad2Deg - 90f;
        float current = transform.eulerAngles.z;
        float next = Mathf.MoveTowardsAngle(current, targetAngle, faceTurnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, next);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 p = transform.position;
        Gizmos.DrawWireCube(p, Vector3.one * checkBoxHalfExtent * 2f);
    }
}