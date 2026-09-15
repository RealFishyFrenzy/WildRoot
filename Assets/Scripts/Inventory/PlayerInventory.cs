using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private InventorySlot[] slots = new InventorySlot[20];

    public InventorySlot[] Slots => slots;
    public event Action Changed;

    // Move/swap whole contents, keeping slot objects and quantities intact.
    public bool TryMoveOrSwap(int sourceIndex, int destinationIndex)
    {
        InventorySlot source = GetSlot(sourceIndex);
        InventorySlot destination = GetSlot(destinationIndex);
        if (sourceIndex == destinationIndex || source == null || destination == null || source.IsEmpty)
            return false;

        var previous = new InventorySlot();
        previous.CopyContentsFrom(destination);
        destination.CopyContentsFrom(source);
        source.CopyContentsFrom(previous);
        Changed?.Invoke();
        return true;
    }

    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                slots[i] = new InventorySlot();
        }
    }

    // All-or-nothing addition. Use AddItemPartial for world pickups.
    public bool AddItem(ItemData item, int amount = 1)
    {
        if (!IsValidAmount(item, amount))
            return false;

        InventorySlot[] plannedSlots = CopySlots();
        if (AddToSlots(plannedSlots, item, amount) != amount)
            return false;

        Commit(plannedSlots);
        return true;
    }

    // Returns the quantity accepted; the caller keeps the remainder.
    public int AddItemPartial(ItemData item, int amount)
    {
        if (!IsValidAmount(item, amount))
            return 0;

        int accepted = AddToSlots(slots, item, amount);
        if (accepted > 0)
            Changed?.Invoke();
        return accepted;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0 || GetQuantity(item) < amount)
            return false;

        RemoveFromSlots(slots, item, amount);
        Changed?.Invoke();
        return true;
    }

    public int GetQuantity(ItemData item)
    {
        return item == null ? 0 : CountInSlots(slots, item);
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        return item != null && amount > 0 && GetQuantity(item) >= amount;
    }

    public InventorySlot GetSlot(int index)
    {
        return index < 0 || index >= slots.Length ? null : slots[index];
    }

    // Match the captured instance, not an ItemData or a slot that may have moved.
    public bool RecordToolUse(ToolInstance instance, bool succeeded)
    {
        if (instance == null || !succeeded)
            return false;
        foreach (InventorySlot slot in slots)
        {
            if (!ReferenceEquals(slot.ToolState, instance))
                continue;
            if (!instance.RecordUse(succeeded))
                return false;

            if (instance.IsBroken)
            {
                slot.Clear();
            }

            Changed?.Invoke();
            return true;
        }
        return false;
    }

    public bool CanExchange(
        IReadOnlyDictionary<ItemData, int> ingredients, ItemData output, int outputAmount)
    {
        return TryPlanExchange(ingredients, output, outputAmount, out _);
    }

    public bool TryExchange(
        IReadOnlyDictionary<ItemData, int> ingredients, ItemData output, int outputAmount)
    {
        if (!TryPlanExchange(ingredients, output, outputAmount, out InventorySlot[] plannedSlots))
            return false;

        Commit(plannedSlots);
        return true;
    }

    private bool TryPlanExchange(
        IReadOnlyDictionary<ItemData, int> ingredients, ItemData output, int outputAmount,
        out InventorySlot[] plannedSlots)
    {
        plannedSlots = null;
        if (ingredients == null || ingredients.Count == 0 || !IsValidAmount(output, outputAmount))
            return false;

        plannedSlots = CopySlots();
        foreach (KeyValuePair<ItemData, int> ingredient in ingredients)
        {
            if (ingredient.Key == null || ingredient.Value <= 0 ||
                CountInSlots(plannedSlots, ingredient.Key) < ingredient.Value)
                return false;

            RemoveFromSlots(plannedSlots, ingredient.Key, ingredient.Value);
        }

        // Check space after consuming ingredients, including newly freed slots.
        return AddToSlots(plannedSlots, output, outputAmount) == outputAmount;
    }

    private static bool IsValidAmount(ItemData item, int amount)
    {
        return item != null && amount > 0 && (item is ToolItem || !item.stackable || item.maxStack > 0);
    }

    private InventorySlot[] CopySlots()
    {
        InventorySlot[] copy = new InventorySlot[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            copy[i] = new InventorySlot();
            copy[i].CopyContentsFrom(slots[i]);
        }
        return copy;
    }

    private void Commit(InventorySlot[] plannedSlots)
    {
        // Preserve slot references held by existing scene/UI code.
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].CopyContentsFrom(plannedSlots[i]);
        }
        Changed?.Invoke();
    }

    private static int CountInSlots(InventorySlot[] targetSlots, ItemData item)
    {
        long total = 0;
        foreach (InventorySlot slot in targetSlots)
        {
            if (slot.item == item)
                total += slot.quantity;
        }
        return (int)Math.Min(total, int.MaxValue);
    }

    private static int AddToSlots(InventorySlot[] targetSlots, ItemData item, int amount)
    {
        int remaining = amount;
        bool stackable = item.stackable && !(item is ToolItem);
        if (stackable)
        {
            foreach (InventorySlot slot in targetSlots)
            {
                if (slot.item != item || slot.quantity >= item.maxStack)
                    continue;

                int added = Mathf.Min(item.maxStack - slot.quantity, remaining);
                slot.quantity += added;
                remaining -= added;
                if (remaining == 0)
                    return amount;
            }
        }

        foreach (InventorySlot slot in targetSlots)
        {
            if (!slot.IsEmpty)
                continue;

            int added = stackable ? Mathf.Min(item.maxStack, remaining) : 1;
            slot.item = item;
            slot.quantity = added;
            remaining -= added;
            if (remaining == 0)
                break;
        }
        return amount - remaining;
    }

    private static void RemoveFromSlots(InventorySlot[] targetSlots, ItemData item, int amount)
    {
        for (int i = targetSlots.Length - 1; i >= 0 && amount > 0; i--)
        {
            InventorySlot slot = targetSlots[i];
            if (slot.item != item)
                continue;

            int removed = Mathf.Min(slot.quantity, amount);
            slot.quantity -= removed;
            amount -= removed;
            if (slot.quantity == 0)
                slot.Clear();
        }
    }
}
