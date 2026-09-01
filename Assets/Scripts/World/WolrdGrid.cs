using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    public static WorldGrid Instance { get; private set; }

    [SerializeField] private float cellSize = 1f;

    public float CellSize => cellSize;

    private void Awake()
    {
        Instance = this;
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);

        return new Vector2Int(x, y);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = (cell.x * cellSize) + (cellSize / 2f);
        float y = (cell.y * cellSize) + (cellSize / 2f);

        return new Vector3(x, y, 0f);
    }
}