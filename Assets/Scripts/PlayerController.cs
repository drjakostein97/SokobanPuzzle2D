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

        state.playerPos = targetPos;
        StartCoroutine(MovePlayer(targetPos));
    }

    IEnumerator MovePlayer(Vector2Int newGridPos)
    {
        isMoving = true;
        yield return StartCoroutine(MoveObject(transform, levelManager.GridToWorld(newGridPos), moveDuration));
        isMoving = false;

        if (levelManager.state.CheckWin())
        {
            Debug.Log("Level Complete!");
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