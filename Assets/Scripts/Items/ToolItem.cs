using UnityEngine;

[CreateAssetMenu(fileName = "NewTool", menuName = "Items/Tool")]
public class ToolItem : ItemData
{
    [Header("Tool")]
    public ToolType toolType;

    [SerializeField] private int power = 1;

    [Header("Progression (independent of power)")]
    [SerializeField] private ToolTier tier;
    [SerializeField] private bool unbreakable = true;
    [SerializeField, Min(0)] private int maximumDurability;
    [SerializeField, Min(0)] private int durabilityCostPerUse;

    public int Power => power;
    public ToolTier Tier => tier;
    public bool Unbreakable => unbreakable;
    public int MaximumDurability => System.Math.Max(0, maximumDurability);
    public int DurabilityCostPerUse => System.Math.Max(0, durabilityCostPerUse);
    public bool IsGatheringTool => toolType == ToolType.Axe ||
        toolType == ToolType.Pickaxe || toolType == ToolType.Shovel;

    public override bool Use(GameObject target)
    {
        if (target == null)
            return false;

        ToolTarget toolTarget =
            target.GetComponentInParent<ToolTarget>();

        if (toolTarget != null)
        {
            return toolTarget.UseTool(toolType, power);
        }

        return false;
    }
}
