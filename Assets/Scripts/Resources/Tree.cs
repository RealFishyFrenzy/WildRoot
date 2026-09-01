using UnityEngine;

public class Tree : ToolTarget
{
    [Header("Tree Drop")]
    [SerializeField] private ItemData woodItem;
    [SerializeField] private int woodAmount = 3;

    [Header("World Drop")]
    [SerializeField] private WorldItemDrop worldItemDropPrefab;

    protected override void OnDestroyed()
    {
        if (woodItem != null && worldItemDropPrefab != null)
        {
            for (int i = 0; i < woodAmount; i++)
            {
                WorldItemDrop drop =
                    Instantiate(
                        worldItemDropPrefab,
                        transform.position,
                        Quaternion.identity
                    );

                drop.Setup(woodItem, 1);
            }
        }

        Debug.Log(
            $"Tree chopped down! Dropped {woodAmount} wood."
        );

        Destroy(gameObject);
    }
}