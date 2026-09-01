using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance { get; private set; }

    [Header("Tilemap")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Terrain Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase sandTile;

    private void Awake()
    {
        Instance = this;
    }

    public TerrainType GetTerrainType(Vector3 worldPosition)
    {
        Vector3Int cellPosition =
            groundTilemap.WorldToCell(worldPosition);

        TileBase tile =
            groundTilemap.GetTile(cellPosition);

        if (tile == grassTile)
            return TerrainType.Grass;

        if (tile == sandTile)
            return TerrainType.Sand;

        return TerrainType.None;
    }
}