using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private HotbarUI hotbarUI;
    [SerializeField] private NetUI netUI;

    private void Update()
    {
        if (!PlayerController.Instance.ControlsEnabled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            // Don't use/place anything when clicking UI.
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (netUI != null && netUI.HasAnimalSelected)
            {
                Vector3 mousePosition =
                    Camera.main.ScreenToWorldPoint(Input.mousePosition);

                mousePosition.z = 0f;

                netUI.TryPlaceSelectedAnimal(mousePosition);

                return;
            }

            UseSelectedItem();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleGuide();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ItemData selectedItem = hotbar.SelectedItem;

            if (selectedItem is ToolItem toolItem &&
                toolItem.toolType == ToolType.Net)
            {
                netUI.Toggle();
            }
        }
    }

    private void UseSelectedItem()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        ItemData item = hotbar.SelectedItem;

        if (item == null)
            return;

        Vector3 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        mousePosition.z = 0f;

        Collider2D[] targets =
        Physics2D.OverlapPointAll(mousePosition);

        bool itemWasUsed = false;

        // Try every GameObject under the mouse until
        // something successfully handles the item.
        foreach (Collider2D target in targets)
        {
            if (item.Use(target.gameObject))
            {
                itemWasUsed = true;
                break;
            }
        }

        // If nothing handled the tool, try using it on terrain.
        if (!itemWasUsed && item is ToolItem tool)
        {
            itemWasUsed =
                TerrainToolSystem.Instance.UseTool(tool, mousePosition);
        }

        if (itemWasUsed && item.consumable)
        {
            inventory.RemoveItem(item, 1);
            hotbarUI.Refresh();
        }
    }

    private void ToggleInventory()
    {
        Debug.Log("Inventory toggled.");
    }

    private void ToggleGuide()
    {
        Debug.Log("Guide toggled.");
    }
}