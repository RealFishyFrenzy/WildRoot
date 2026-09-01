using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;

    [Header("Inventory")]
    public bool stackable = true;
    public int maxStack = 999;

    [Header("Usage")]
    public bool consumable;

    public virtual bool Use(GameObject target)
    {
        Debug.Log($"{itemName} cannot be used here.");
        return false;
    }
}