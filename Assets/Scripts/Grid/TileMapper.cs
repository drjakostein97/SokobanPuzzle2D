using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TileMapper : MonoBehaviour
{
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap targetTilemap;

    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase targetTile;

    public void RenderGrid(SokobanGrid grid)
    {
        Debug.Log($"Grid size: {grid.width} x {grid.height}");   // <-- new

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                var cellPos = new Vector3Int(x, y, 0);
                Debug.Log($"Placing tile at {cellPos}, type: {grid.cells[x, y]}");   // <-- new

                var cellType = grid.cells[x, y];

                if (cellType != CellType.Wall)
                    floorTilemap.SetTile(cellPos, floorTile);

                if (cellType == CellType.Wall)
                    wallTilemap.SetTile(cellPos, wallTile);

                if (cellType == CellType.Target)
                    targetTilemap.SetTile(cellPos, targetTile);
            }
        }
    }
}