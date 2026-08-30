using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TileMapper tileMapper;

    private SokobanGrid grid;
    private GameState state;

    void Start()
    {
        var levelText = new string[] { "#####", "#. $#", "#@  #", "#####" };
        (grid, state) = LevelParser.ParseLevel(levelText);

        tileMapper.RenderGrid(grid);
    }
}
