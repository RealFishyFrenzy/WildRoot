using UnityEngine;

public class TerrainToolSystem : MonoBehaviour
{
    public static TerrainToolSystem Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private ItemData sandItem;
    [SerializeField] private ItemData dirtItem;

    [Header("World Drops")]
    [SerializeField] private WorldItemDrop worldItemDropPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public bool UseTool(ToolItem tool, Vector3 worldPosition)
    {
        if (tool == null || tool.toolType != ToolType.Shovel)
            return false;

        TerrainType terrain =
            TerrainManager.Instance.GetTerrainType(worldPosition);

        switch (terrain)
        {
            case TerrainType.Sand:
                SpawnDrop(sandItem, worldPosition);
                Debug.Log("Dug up Sand!");
                return true;

            case TerrainType.Grass:
                SpawnDrop(dirtItem, worldPosition);
                Debug.Log("Dug up Dirt!");
                return true;
        }

        return false;
    }

    private void SpawnDrop(ItemData item, Vector3 position)
    {
        if (item == null || worldItemDropPrefab == null)
            return;

        WorldItemDrop drop =
            Instantiate(worldItemDropPrefab, position, Quaternion.identity);

        drop.Setup(item, 1);
    }
}