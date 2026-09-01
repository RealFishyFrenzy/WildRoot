using UnityEngine;

public class Rock : ToolTarget
{
    [Header("Rock Drop")]
    [SerializeField] private ItemData stoneItem;
    [SerializeField] private int stoneAmount = 3;

    [Header("World Drop")]
    [SerializeField] private WorldItemDrop worldItemDropPrefab;

    protected override void OnDestroyed()
    {
        Debug.Log("Rock destroyed.");

        if (stoneItem == null)
        {
            Debug.LogWarning("Stone Item is NOT assigned.");
            return;
        }

        if (worldItemDropPrefab == null)
        {
            Debug.LogWarning("World Item Drop Prefab is NOT assigned.");
            return;
        }

        Debug.Log($"Spawning {stoneAmount} stone drops.");

        for (int i = 0; i < stoneAmount; i++)
        {
            WorldItemDrop drop =
                Instantiate(
                    worldItemDropPrefab,
                    transform.position,
                    Quaternion.identity
                );

            drop.Setup(stoneItem, 1);
        }

        Destroy(gameObject);
    }
}