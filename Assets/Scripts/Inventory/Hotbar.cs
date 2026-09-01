using UnityEngine;

public class Hotbar : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private int hotbarSize = 5;

    public int SelectedSlot { get; private set; }

    public InventorySlot SelectedInventorySlot
    {
        get
        {
            if (inventory == null)
                return null;

            return inventory.GetSlot(SelectedSlot);
        }
    }

    public ItemData SelectedItem
    {
        get
        {
            InventorySlot slot = SelectedInventorySlot;

            if (slot == null || slot.IsEmpty)
                return null;

            return slot.item;
        }
    }

    private void Update()
    {
        if (!PlayerController.Instance.ControlsEnabled)
            return;

        HandleNumberKeys();
        HandleMouseWheel();
    }

    private void HandleNumberKeys()
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    private void HandleMouseWheel()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0)
        {
            SelectSlot(SelectedSlot - 1);
        }
        else if (scroll < 0)
        {
            SelectSlot(SelectedSlot + 1);
        }
    }

    private void SelectSlot(int index)
    {
        if (index < 0)
            index = hotbarSize - 1;

        if (index >= hotbarSize)
            index = 0;

        SelectedSlot = index;

        if (SelectedItem != null)
        {
            Debug.Log($"Selected: {SelectedItem.itemName}");
        }
        else
        {
            Debug.Log($"Selected empty slot {SelectedSlot + 1}");
        }
    }
}