using System;
using UnityEngine;

// Owned by one inventory entry, never by the shared ToolItem asset.
[Serializable]
public sealed class ToolInstance
{
    [SerializeField] private ToolItem definition;
    [SerializeField] private int currentDurability;

    public ToolItem Definition => definition;
    public bool Unbreakable => definition != null && definition.Unbreakable;
    public int MaximumDurability => definition != null ? definition.MaximumDurability : 0;
    public int CurrentDurability => Math.Max(0, Math.Min(currentDurability, MaximumDurability));
    public bool IsBroken => definition != null && !Unbreakable && CurrentDurability == 0;
    public bool CanUse => definition != null && (Unbreakable ||
        (CurrentDurability > 0 && definition.DurabilityCostPerUse > 0));

    public ToolInstance(ToolItem definition)
    {
        this.definition = definition;
        currentDurability = definition != null && !definition.Unbreakable
            ? definition.MaximumDurability : 0;
    }

    public bool RecordUse(bool succeeded)
    {
        if (!succeeded || !CanUse || Unbreakable || !definition.IsGatheringTool)
            return false;

        int next = Math.Max(0, CurrentDurability - definition.DurabilityCostPerUse);
        bool changed = next != CurrentDurability;
        currentDurability = next;
        return changed;
    }
}
