using UnityEngine;

public class SokobanGrid
{
    public CellType[,] cells;
    public int width, height;

    public CellType GetCell(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return CellType.Wall; // treat out-of-bounds as a wall
        return cells[pos.x, pos.y];
    }

    public bool IsWalkable(Vector2Int pos)
    {
        return GetCell(pos) != CellType.Wall;
    }
}
