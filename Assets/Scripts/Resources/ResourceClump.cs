using UnityEngine;

public class ResourceNode : ToolTarget
{
    [Header("Drops")]
    [SerializeField] private ResourceDrop[] drops;

    [Header("World Drop")]
    [SerializeField] private WorldItemDrop worldItemDropPrefab;

    protected override void OnDestroyed()
    {
        if (drops == null || worldItemDropPrefab == null)
        {
            Destroy(gameObject);
            return;
        }

        foreach (ResourceDrop dropData in drops)
        {
            if (dropData == null || dropData.item == null)
                continue;

            float roll = Random.value;

            if (roll > dropData.dropChance)
                continue;

            int amount = Random.Range(
                dropData.minAmount,
                dropData.maxAmount + 1
            );

            for (int i = 0; i < amount; i++)
            {
                WorldItemDrop drop =
                    Instantiate(
                        worldItemDropPrefab,
                        transform.position,
                        Quaternion.identity
                    );

                drop.Setup(dropData.item, 1);
            }
        }

        Destroy(gameObject);
    }
}