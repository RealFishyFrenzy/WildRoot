using UnityEngine;

[CreateAssetMenu(fileName = "NewTool", menuName = "Items/Tool")]
public class ToolItem : ItemData
{
    [Header("Tool")]
    public ToolType toolType;

    [SerializeField] private int power = 1;

    public override bool Use(GameObject target)
    {
        ToolTarget toolTarget =
            target.GetComponentInParent<ToolTarget>();

        if (toolTarget != null)
        {
            return toolTarget.UseTool(toolType, power);
        }

        return false;
    }
}