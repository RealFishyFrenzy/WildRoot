using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Xml.Serialization;

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

    private int lastSelectedSlot = -1;

    private void Start()
    {
        selectedItemText.gameObject.SetActive(false);
        Refresh();
    }

    private void Update()
    {
        if (hotbar.SelectedSlot == lastSelectedSlot)
            return;

        lastSelectedSlot = hotbar.SelectedSlot;

        Refresh();
        ShowSelectedItemText();
    }

    public void Refresh()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            slotImages[i].color =
                i == hotbar.SelectedSlot
                ? selectedColor
                : normalColor;

            InventorySlot slot = inventory.GetSlot(i);

            if (slot != null && !slot.IsEmpty)
            {
                itemIcons[i].sprite = slot.item.icon;
                itemIcons[i].enabled = true;

                if (slot.item.stackable && slot.quantity > 1)
                {
                    quantityTexts[i].text = slot.quantity.ToString();
                }
                else
                {
                    quantityTexts[i].text = "";
                }
            }
            else
            {
                itemIcons[i].sprite = null;
                itemIcons[i].enabled = false;
                quantityTexts[i].text = "";
            }
        }
    }

    private Coroutine itemTextRoutine;


    private void ShowSelectedItemText()
    {
        if (itemTextRoutine != null)
            StopCoroutine(itemTextRoutine);

        if (hotbar.SelectedItem != null)
            selectedItemText.text = hotbar.SelectedItem.itemName;
        else
            selectedItemText.text = "Empty";

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