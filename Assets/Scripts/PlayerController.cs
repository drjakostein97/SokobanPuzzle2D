using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public LevelManager levelManager;
    public float moveDuration = 0.15f;

    private bool isMoving = false;

    void Update()
    {
        if (isMoving) return; // ignore input mid-move
        if (Keyboard.current == null) return; // no keyboard connected

        Vector2Int direction = Vector2Int.zero;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame) direction = Vector2Int.up;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame) direction = Vector2Int.down;
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame) direction = Vector2Int.left;
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            TryMove(direction);
        }
    }

    void TryMove(Vector2Int direction)
    {
        var grid = levelManager.grid;
        var state = levelManager.state;

        Vector2Int targetPos = state.playerPos + direction;

        if (!grid.IsWalkable(targetPos)) return;

        int boxIndex = state.boxPositions.IndexOf(targetPos);

        if (boxIndex != -1)
        {
            Vector2Int boxTargetPos = targetPos + direction;

            if (!grid.IsWalkable(boxTargetPos)) return;
            if (state.boxPositions.Contains(boxTargetPos)) return;

            state.boxPositions[boxIndex] = boxTargetPos;

            GameObject boxObj = levelManager.boxInstances[boxIndex];
            StartCoroutine(MoveObject(boxObj.transform, levelManager.GridToWorld(boxTargetPos), moveDuration));
        }

        // Rotate to face the movement direction
        transform.rotation = Quaternion.Euler(0, 0, GetRotationZ(direction));

        state.playerPos = targetPos;
        StartCoroutine(MovePlayer(targetPos));
    }

    float GetRotationZ(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 0f;
        if (direction == Vector2Int.down) return 180f;
        if (direction == Vector2Int.left) return 90f;
        if (direction == Vector2Int.right) return -90f;
        return 0f;
    }

    IEnumerator MovePlayer(Vector2Int newGridPos)
    {
        isMoving = true;
        yield return StartCoroutine(MoveObject(transform, levelManager.GridToWorld(newGridPos), moveDuration));
        isMoving = false;

        Debug.Log($"Boxes: {string.Join(", ", levelManager.state.boxPositions)}");
        Debug.Log($"Targets: {string.Join(", ", levelManager.state.targetPositions)}");

        if (levelManager.state.CheckWin())
        {
            Debug.Log("Level Complete!");
            levelManager.PlayWinConfetti();
        }
    }

    IEnumerator MoveObject(Transform obj, Vector3 targetWorldPos, float duration)
    {
        Vector3 startPos = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.position = Vector3.Lerp(startPos, targetWorldPos, elapsed / duration);
            yield return null;
        }

        obj.position = targetWorldPos;
    }
}