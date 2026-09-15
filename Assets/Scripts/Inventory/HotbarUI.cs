using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private Image[] slotImages;
    [SerializeField] private Image[] itemIcons;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [SerializeField] private TMP_Text selectedItemText;
    [SerializeField] private float textDisplayTime = 1.5f;

    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private TMP_Text[] quantityTexts;
    [SerializeField] private GameObject[] durabilityBars;
    [SerializeField] private Image[] durabilityFills;

    private int lastSelectedSlot = -1;
    private ItemData lastSelectedItem;

    private void OnEnable()
    {
        inventory.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inventory.Changed -= Refresh;
    }

    private void Start()
    {
        selectedItemText.gameObject.SetActive(false);
        Refresh();
    }

    private void Update()
    {
        if (hotbar.SelectedSlot == lastSelectedSlot && hotbar.SelectedItem == lastSelectedItem)
            return;

        lastSelectedSlot = hotbar.SelectedSlot;
        lastSelectedItem = hotbar.SelectedItem;

        Refresh();
        ShowSelectedItemText();
    }

    public void Refresh()
    {
        if (slotImages == null || inventory == null) return;

        EnsureDurabilityArrays();

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                bool isSelected = (hotbar != null && i == hotbar.SelectedSlot);
                slotImages[i].sprite = isSelected ? WildRootUITheme.SlotSelected : WildRootUITheme.SlotHotbar;
                slotImages[i].type = UnityEngine.UI.Image.Type.Sliced;
                slotImages[i].color = Color.white;
            }

            InventorySlot slot = inventory.GetSlot(i);

            if (itemIcons != null && i < itemIcons.Length && itemIcons[i] != null)
            {
                if (slot != null && !slot.IsEmpty && slot.item != null)
                {
                    itemIcons[i].sprite = slot.item.icon;
                    itemIcons[i].enabled = true;
                    itemIcons[i].preserveAspect = true;
                }
                else
                {
                    itemIcons[i].sprite = null;
                    itemIcons[i].enabled = false;
                }
            }

            if (quantityTexts != null && i < quantityTexts.Length && quantityTexts[i] != null)
            {
                UpdateQuantityText(quantityTexts[i], slot);
            }

            if (durabilityBars != null && i < durabilityBars.Length &&
                durabilityFills != null && i < durabilityFills.Length)
            {
                WildRootUITheme.UpdateDurabilityBar(durabilityBars[i], durabilityFills[i], slot);
            }
        }
    }

    private void EnsureDurabilityArrays()
    {
        if (slotImages == null) return;

        if (durabilityBars == null || durabilityBars.Length != slotImages.Length ||
            durabilityFills == null || durabilityFills.Length != slotImages.Length)
        {
            durabilityBars = new GameObject[slotImages.Length];
            durabilityFills = new UnityEngine.UI.Image[slotImages.Length];

            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] == null) continue;

                Transform existing = slotImages[i].transform.Find("DurabilityBar");
                if (existing != null)
                {
                    durabilityBars[i] = existing.gameObject;
                }
                else
                {
                    durabilityBars[i] = WildRootUITheme.CreateDurabilityBar(
                        slotImages[i].transform,
                        new Vector2(56f, 6f),
                        new Vector2(0f, 8f));
                }

                if (durabilityBars[i] != null)
                {
                    Transform fillT = durabilityBars[i].transform.Find("Fill");
                    if (fillT != null)
                        durabilityFills[i] = fillT.GetComponent<UnityEngine.UI.Image>();
                }
            }
        }
    }

    private Coroutine itemTextRoutine;

    private void ShowSelectedItemText()
    {
        if (selectedItemText == null) return;

        if (itemTextRoutine != null)
            StopCoroutine(itemTextRoutine);

        if (hotbar != null && hotbar.SelectedItem != null)
            selectedItemText.text = $"<b><color=#FFE8A3>{hotbar.SelectedItem.itemName}</color></b>";
        else
            selectedItemText.text = "<color=#A39482>Empty</color>";

        selectedItemText.gameObject.SetActive(true);

        itemTextRoutine = StartCoroutine(HideSelectedItemText());
    }

    private IEnumerator HideSelectedItemText()
    {
        yield return new WaitForSeconds(textDisplayTime);

        selectedItemText.gameObject.SetActive(false);
    }

    private void UpdateQuantityText(
        TMP_Text quantityText,
        InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty)
        {
            quantityText.text = "";
            return;
        }

        if (!slot.item.stackable || slot.quantity <= 1)
        {
            quantityText.text = "";
            return;
        }

        quantityText.text = slot.quantity.ToString();
    }
}
