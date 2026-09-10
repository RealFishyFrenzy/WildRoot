using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private NetUI netUI;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private AnimalInfoUI animalInfoUI;

    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    private void Update()
    {
        // Handle closing before the gameplay gate, otherwise Escape would be blocked too.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (animalInfoUI == null)
                animalInfoUI = AnimalInfoUI.Instance != null ? AnimalInfoUI.Instance : FindAnyObjectByType<AnimalInfoUI>();

            if (animalInfoUI != null && animalInfoUI.IsOpen)
            {
                animalInfoUI.Close();
                return;
            }

            if (inventoryUI == null)
                inventoryUI = InventoryUI.Instance != null ? InventoryUI.Instance : FindAnyObjectByType<InventoryUI>();

            if (inventoryUI != null && inventoryUI.IsOpen)
                inventoryUI.Close();
            else if (PlayerController.Instance.ControlsEnabled)
                ToggleInventory();

            return;
        }

        if (!PlayerController.Instance.ControlsEnabled)
            return;

        hotbar.HandleSelectionInput();

        if (!(hotbar.SelectedItem is ToolItem selectedTool && selectedTool.toolType == ToolType.Net))
            netUI?.CancelPlacement();

        // A single dispatcher owns interaction input: E always interacts;
        // right-click opens the net when equipped, otherwise it interacts.
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerInteraction?.TryInteract();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (hotbar.SelectedItem is ToolItem toolItem && toolItem.toolType == ToolType.Net)
            {
                netUI?.Toggle();
            }
            else
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0f;
                Collider2D[] targets = Physics2D.OverlapPointAll(mousePosition);
                Animal clickedAnimal = null;

                foreach (Collider2D target in targets)
                {
                    clickedAnimal = target.GetComponentInParent<Animal>();
                    if (clickedAnimal != null)
                        break;
                }

                if (clickedAnimal != null)
                {
                    if (animalInfoUI == null)
                        animalInfoUI = AnimalInfoUI.Instance != null ? AnimalInfoUI.Instance : FindAnyObjectByType<AnimalInfoUI>();

                    animalInfoUI?.Open(clickedAnimal);
                }
                else
                {
                    playerInteraction?.TryInteract();
                }
            }

            return;
        }

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

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleGuide();
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
        }
    }

    private void ToggleInventory()
    {
        if (inventoryUI == null)
            inventoryUI = FindAnyObjectByType<InventoryUI>();

        if (inventoryUI != null)
        {
            inventoryUI.Toggle();
        }
        else
        {
            Debug.Log("Inventory toggled.");
        }
    }

    private void ToggleGuide()
    {
        Debug.Log("Guide toggled.");
    }
}
