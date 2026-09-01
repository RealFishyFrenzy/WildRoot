using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private InventorySlot[] slots = new InventorySlot[20];

    public InventorySlot[] Slots => slots;

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new InventorySlot();
        }
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        // First try adding to existing stacks.
        if (item.stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                InventorySlot slot = slots[i];

                if (slot.item != item)
                    continue;

                int spaceRemaining = item.maxStack - slot.quantity;

                if (spaceRemaining <= 0)
                    continue;

                int amountToAdd = Mathf.Min(spaceRemaining, amount);

                slot.quantity += amountToAdd;
                amount -= amountToAdd;

                if (amount <= 0)
                    return true;
            }
        }

        // Then use empty slots.
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty)
                continue;

            slots[i].item = item;

            if (item.stackable)
            {
                int amountToAdd = Mathf.Min(item.maxStack, amount);

                slots[i].quantity = amountToAdd;
                amount -= amountToAdd;
            }
            else
            {
                slots[i].quantity = 1;
                amount--;
            }

            if (amount <= 0)
                return true;
        }

        // Not everything fit.
        return false;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        if (GetQuantity(item) < amount)
            return false;

        for (int i = slots.Length - 1; i >= 0; i--)
        {
            InventorySlot slot = slots[i];

            if (slot.item != item)
                continue;

            int amountToRemove = Mathf.Min(slot.quantity, amount);

            slot.quantity -= amountToRemove;
            amount -= amountToRemove;

            if (slot.quantity <= 0)
                slot.Clear();

            if (amount <= 0)
                return true;
        }

        return true;
    }

    public int GetQuantity(ItemData item)
    {
        int total = 0;

        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item)
                total += slot.quantity;
        }

        return total;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        return GetQuantity(item) >= amount;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }
}