using UnityEngine;

public static class LevelParser
{
    public static (SokobanGrid, GameState) ParseLevel(string[] lines)
    {
        var grid = new SokobanGrid { width = lines[0].Length, height = lines.Length };
        grid.cells = new CellType[grid.width, grid.height];
        var state = new GameState { boxPositions = new(), targetPositions = new() };

        for (int y = 0; y < lines.Length; y++)
        {
            for (int x = 0; x < lines[y].Length; x++)
            {
                char c = lines[y][x];
                int flippedY = grid.height - 1 - y;   // <-- flip here
                var pos = new Vector2Int(x, flippedY);

                grid.cells[x, flippedY] = c switch
                {
                    '#' => CellType.Wall,
                    '.' or '*' or '+' => CellType.Target,
                    _ => CellType.Floor
                };

                if (c is '$' or '*') state.boxPositions.Add(pos);
                if (c is '.' or '*' or '+') state.targetPositions.Add(pos);
                if (c is '@' or '+') state.playerPos = pos;
            }
        }

        return (grid, state);
    }
}