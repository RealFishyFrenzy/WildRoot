using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;

    public void SetSlot(InventorySlot slot)
    {
        if (slot != null && !slot.IsEmpty && slot.item != null)
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = slot.item.icon;
                itemIcon.enabled = true;
            }

            if (quantityText != null)
            {
                if (slot.item.stackable && slot.quantity > 1)
                {
                    quantityText.text = slot.quantity.ToString();
                }
                else
                {
                    quantityText.text = "";
                }
            }
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
        }
    }
}
