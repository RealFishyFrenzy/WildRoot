using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject durabilityBar;
    [SerializeField] private Image durabilityFill;

    private InventoryUI owner;
    private int slotIndex;
    private Outline selectionOutline;

    public void Bind(InventoryUI inventoryUI, int index)
    {
        owner = inventoryUI;
        slotIndex = index;

        UnityEngine.UI.Image background = GetComponent<UnityEngine.UI.Image>();
        if (background != null)
        {
            background.raycastTarget = true;
            background.sprite = (index < 10) ? WildRootUITheme.SlotHotbar : WildRootUITheme.SlotNormal;
            background.type = UnityEngine.UI.Image.Type.Sliced;
            background.color = Color.white;

            selectionOutline = GetComponent<Outline>();
            if (selectionOutline == null)
                selectionOutline = gameObject.AddComponent<Outline>();
            selectionOutline.effectColor = WildRootUITheme.ColorGoldAmber;
            selectionOutline.effectDistance = new Vector2(3f, -3f);
            SetSelected(false);
        }

        if (quantityText != null)
        {
            quantityText.font = TMP_Settings.defaultFontAsset;
            quantityText.fontStyle = FontStyles.Bold;
            quantityText.fontSize = 16f;
            quantityText.color = WildRootUITheme.ColorTextBody;
            quantityText.alignment = TextAlignmentOptions.BottomRight;
            quantityText.raycastTarget = false;
        }

        if (itemIcon != null)
        {
            itemIcon.preserveAspect = true;
            itemIcon.raycastTarget = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            owner?.SelectSlot(slotIndex);
    }

    public void SetSelected(bool selected)
    {
        if (selectionOutline != null)
            selectionOutline.enabled = selected;
    }

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

            UpdateDurability(slot);
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

        if (durabilityBar != null)
        {
            durabilityBar.SetActive(false);
        }
    }

    private void UpdateDurability(InventorySlot slot)
    {
        EnsureDurabilityComponents();
        WildRootUITheme.UpdateDurabilityBar(durabilityBar, durabilityFill, slot);
    }

    private void EnsureDurabilityComponents()
    {
        if (durabilityBar == null)
        {
            Transform existing = transform.Find("DurabilityBar");
            if (existing != null)
            {
                durabilityBar = existing.gameObject;
            }
            else
            {
                durabilityBar = WildRootUITheme.CreateDurabilityBar(transform, new Vector2(48f, 5f), new Vector2(0f, 6f));
            }
        }

        if (durabilityFill == null && durabilityBar != null)
        {
            Transform fillT = durabilityBar.transform.Find("Fill");
            if (fillT != null)
                durabilityFill = fillT.GetComponent<UnityEngine.UI.Image>();
        }
    }
}
