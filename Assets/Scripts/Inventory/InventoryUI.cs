using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    public PlayerInventory Inventory => playerInventory;

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventorySlotUI[] slotUIs;
    [SerializeField] private PlayerMenuUI playerMenu;

    public bool IsOpen => playerMenu != null ? playerMenu.IsOpen :
        inventoryPanel != null && inventoryPanel.activeInHierarchy;
    private int selectedSource = -1;

    public void SelectSlot(int index)
    {
        if (!IsOpen || (playerMenu != null && !playerMenu.IsPlayerTab) ||
            playerInventory == null || playerInventory.GetSlot(index) == null)
            return;

        if (selectedSource == index)
            selectedSource = -1;
        else if (selectedSource < 0)
        {
            if (!playerInventory.GetSlot(index).IsEmpty)
                selectedSource = index;
        }
        else
        {
            int source = selectedSource;
            selectedSource = -1;
            playerInventory.TryMoveOrSwap(source, index);
        }
        Refresh();
    }

    private void Awake()
    {
        Instance = this;

        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        if (slotUIs != null)
            for (int i = 0; i < slotUIs.Length; i++)
                if (slotUIs[i] != null)
                    slotUIs[i].Bind(this, i);

        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();

        if (playerInventory != null)
            playerInventory.Changed += RefreshIfOpen;

        RefreshIfOpen();
    }

    private void OnDisable()
    {
        if (playerMenu != null)
            playerMenu.Hide();
        selectedSource = -1;
        if (playerInventory != null)
            playerInventory.Changed -= RefreshIfOpen;
    }

    private void RefreshIfOpen()
    {
        // A pickup/craft may replace the source while the panel is open.
        selectedSource = -1;
        if (IsOpen)
            Refresh();
    }

    private void Start()
    {
        if (playerMenu != null)
            playerMenu.Hide();
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void Open()
    {
        selectedSource = -1;
        if (inventoryPanel != null)
        {
            if (playerMenu == null)
                playerMenu = GetComponent<PlayerMenuUI>();
            if (playerMenu == null)
                playerMenu = gameObject.AddComponent<PlayerMenuUI>();
            playerMenu.Initialize(this, inventoryPanel);
            FindAnyObjectByType<NetUI>()?.CancelPlacement();
            AnimalInfoUI.Instance?.Close();
            EnclosureUI.Instance?.Close();
            playerMenu.Show();
            PlayerController.Instance?.BlockControlsForCurrentFrame();
            Refresh();
        }
    }

    public void Close()
    {
        selectedSource = -1;
        if (inventoryPanel != null)
        {
            if (IsOpen)
                PlayerController.Instance?.BlockControlsForCurrentFrame();

            if (playerMenu != null)
                playerMenu.Hide();
            else
                inventoryPanel.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (inventoryPanel == null)
            return;

        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Refresh()
    {
        if (playerInventory == null)
        {
            playerInventory = FindAnyObjectByType<PlayerInventory>();
        }

        if (playerInventory == null || slotUIs == null)
            return;

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
                continue;

            InventorySlot slot = playerInventory.GetSlot(i);
            slotUIs[i].SetSlot(slot);
            slotUIs[i].SetSelected(i == selectedSource);
        }
    }

    public void CancelSlotSelection()
    {
        selectedSource = -1;
        Refresh();
    }
}
