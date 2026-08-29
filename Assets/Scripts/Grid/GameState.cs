using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public Vector2Int playerPos;
    public List<Vector2Int> boxPositions;
    public List<Vector2Int> targetPositions; // cached from grid for fast win-checks

    public bool IsBoxAt(Vector2Int pos) => boxPositions.Contains(pos);

    public bool CheckWin()
    {
        foreach (var target in targetPositions)
            if (!boxPositions.Contains(target))
                return false;
        return true;
    }
}
