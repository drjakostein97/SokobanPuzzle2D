using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public TileMapper tileMapper;
    public Tilemap referenceTilemap;
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public Sprite[] boxSprites;

    [HideInInspector] public SokobanGrid grid;
    [HideInInspector] public GameState state;
    [HideInInspector] public GameObject playerInstance;
    [HideInInspector] public List<GameObject> boxInstances = new();

    void Start()
    {
        var levelText = new string[] { "#####", "#. $#", "#@  #", "#####" };
        (grid, state) = LevelParser.ParseLevel(levelText);

        tileMapper.RenderGrid(grid);
        SpawnEntities();
    }

    void SpawnEntities()
    {
        Vector3 playerWorldPos = referenceTilemap.CellToWorld(new Vector3Int(state.playerPos.x, state.playerPos.y, 0));
        playerWorldPos += referenceTilemap.cellSize / 2f;
        playerInstance = Instantiate(playerPrefab, playerWorldPos, Quaternion.identity);

        foreach (var boxPos in state.boxPositions)
        {
            Vector3 boxWorldPos = referenceTilemap.CellToWorld(new Vector3Int(boxPos.x, boxPos.y, 0));
            boxWorldPos += referenceTilemap.cellSize / 2f;

            var boxObj = Instantiate(boxPrefab, boxWorldPos, Quaternion.identity);

            if (boxSprites != null && boxSprites.Length > 0)
            {
                var sr = boxObj.GetComponent<SpriteRenderer>();
                sr.sprite = boxSprites[Random.Range(0, boxSprites.Length)];
            }

            boxInstances.Add(boxObj);
        }
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3 world = referenceTilemap.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
        return world + referenceTilemap.cellSize / 2f;
    }
}