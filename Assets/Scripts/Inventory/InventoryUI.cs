using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventorySlotUI[] slotUIs;

    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeInHierarchy;

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
        if (playerInventory == null)
            playerInventory = FindAnyObjectByType<PlayerInventory>();

        if (playerInventory != null)
            playerInventory.Changed += RefreshIfOpen;

        RefreshIfOpen();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.Changed -= RefreshIfOpen;
    }

    private void RefreshIfOpen()
    {
        if (IsOpen)
            Refresh();
    }

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void Open()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            PlayerController.Instance?.BlockControlsForCurrentFrame();
            Refresh();
        }
    }

    public void Close()
    {
        if (inventoryPanel != null)
        {
            if (IsOpen)
                PlayerController.Instance?.BlockControlsForCurrentFrame();

            inventoryPanel.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (inventoryPanel == null)
            return;

        if (inventoryPanel.activeSelf)
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
        }
    }
}
