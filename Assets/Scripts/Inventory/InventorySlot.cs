[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    [UnityEngine.SerializeField] private ToolInstance toolInstance;

    // Lazily supports existing scene slots without rewriting scene serialization.
    public ToolInstance ToolState
    {
        get
        {
            if (!(item is ToolItem tool))
                return toolInstance = null;
            if (toolInstance == null || toolInstance.Definition != tool)
                toolInstance = new ToolInstance(tool);
            return toolInstance;
        }
    }

    public void CopyContentsFrom(InventorySlot source)
    {
        item = source.item;
        quantity = source.quantity;
        toolInstance = source.ToolState;
    }

    public bool IsEmpty => item == null;

    public void Clear()
    {
        item = null;
        quantity = 0;
        toolInstance = null;
    }
}
