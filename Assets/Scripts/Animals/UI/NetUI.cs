using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NetUI : MonoBehaviour
{
    public static NetUI Instance { get; private set; }
    private GameObject dedicatedPanel;
    public bool IsOpen => dedicatedPanel != null && dedicatedPanel.activeInHierarchy;
    private void Awake() => Instance = this;
    private void OnDisable() => Close();
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (dedicatedPanel != null) Destroy(dedicatedPanel);
    }
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button animalButton;
    [SerializeField] private TMP_Text animalButtonText;

    [Header("Animal Storage")]
    [SerializeField] private AnimalInventory animalInventory;

    private AnimalInstance selectedAnimal;
    private Hotbar hotbar;

    public bool HasAnimalSelected => selectedAnimal != null;

    // Legacy panel fields remain serialized so existing scene references are preserved.
    // The dedicated view shares records/presentation, never inventory-menu navigation.
    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);
        // Retire the old first-animal widget without deleting saved scene references.
        if (animalButton != null)
            animalButton.gameObject.SetActive(false);
        if (animalButtonText != null)
            animalButtonText.text = "";
    }

    public AnimalInventory Storage
    {
        get
        {
            if (animalInventory == null)
                animalInventory = FindAnyObjectByType<AnimalInventory>();
            return animalInventory;
        }
    }

    public bool CanSelectAnimal
    {
        get
        {
            if (hotbar == null)
                hotbar = FindAnyObjectByType<Hotbar>();
            return hotbar != null && hotbar.SelectedItem is ToolItem tool &&
                tool.toolType == ToolType.Net;
        }
    }

    public void Toggle()
    {
        if (IsOpen) { CancelPlacement(); return; }
        if (!CanSelectAnimal) return;
        InventoryUI.Instance?.Close();
        AnimalInfoUI.Instance?.Close();
        EnclosureUI.Instance?.Close();
        selectedAnimal = null;
        if (dedicatedPanel == null)
        {
            Canvas canvas = panel != null ? panel.GetComponentInParent<Canvas>() : FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Compact, dedicated panel centered with WildRoot wood frame
            RectTransform root = WildRootUITheme.CreatePanel(canvas.transform, "Dedicated Net UI (Runtime)", WildRootUITheme.PanelFrame);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, 0);
            root.sizeDelta = new Vector2(440, 580);
            dedicatedPanel = root.gameObject;

            // Header Ribbon
            WildRootUITheme.CreateHeaderRibbon(root, "Net Title Ribbon", "CREATURE NET", "Quick Placement & Release");
            RectTransform headerRibbon = root.Find("Net Title Ribbon") as RectTransform;
            if (headerRibbon != null)
            {
                headerRibbon.anchorMin = new Vector2(0.04f, 1f);
                headerRibbon.anchorMax = new Vector2(0.82f, 1f);
                headerRibbon.pivot = new Vector2(0f, 1f);
                headerRibbon.anchoredPosition = new Vector2(0, -12);
                headerRibbon.sizeDelta = new Vector2(0, 48);
            }

            // Close Button (Top-Right)
            Button closeBtn = WildRootUITheme.CreateButton(root, "Close Net Button", "✕", 18f, WildRootUITheme.ButtonClose, WildRootUITheme.ButtonClose);
            RectTransform closeRect = (RectTransform)closeBtn.transform;
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-16, -12);
            closeRect.sizeDelta = new Vector2(48, 48);
            closeBtn.onClick.AddListener(CancelPlacement);

            // Inner Container for Animal List
            RectTransform innerContainer = WildRootUITheme.CreatePanel(root, "Net List Inner", WildRootUITheme.PanelInner);
            innerContainer.anchorMin = Vector2.zero;
            innerContainer.anchorMax = Vector2.one;
            innerContainer.offsetMin = new Vector2(16, 16);
            innerContainer.offsetMax = new Vector2(-16, -68);

            NetStorageListUI list = PlayerMenuLayout.CreateAnimalList(innerContainer);
            list.Bind(this, true);
        }
        dedicatedPanel.SetActive(true);
        dedicatedPanel.transform.SetAsLastSibling();
        PlayerController.Instance?.BlockControlsForCurrentFrame();
    }

    public void Close()
    {
        if (IsOpen) PlayerController.Instance?.BlockControlsForCurrentFrame();
        if (dedicatedPanel != null) dedicatedPanel.SetActive(false);
        if (panel != null)
            panel.SetActive(false);
    }

    public void CancelPlacement()
    {
        selectedAnimal = null;
        Close();
    }

    public bool TrySelectAnimal(AnimalInstance animal)
    {
        if (!IsOpen || (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen) ||
            !CanSelectAnimal || Storage == null || !Storage.Contains(animal))
            return false;

        selectedAnimal = animal;
        Close();
        InventoryUI.Instance?.Close();
        PlayerController.Instance?.BlockControlsForCurrentFrame();
        return true;
    }

    public bool TryPlaceSelectedAnimal(Vector3 worldPosition)
    {
        if (selectedAnimal == null)
            return false;

        if (animalInventory == null)
            animalInventory = FindAnyObjectByType<AnimalInventory>();

        // -------------------------
        // 1. TANK / ENCLOSURE CHECK
        // -------------------------
        if (animalInventory == null || !animalInventory.Animals.Contains(selectedAnimal))
            return false;

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit != null)
        {
            AnimalEnclosure enclosure = hit.GetComponent<AnimalEnclosure>();
            AnimalPlacementZone zone = hit.GetComponent<AnimalPlacementZone>();

            if (enclosure != null || (zone != null && zone.ZoneType == PlacementZoneType.Tank))
            {
                if (enclosure == null && zone != null)
                    enclosure = zone.GetComponent<AnimalEnclosure>();

                if (enclosure != null)
                {
                    if (!enclosure.AddAnimal(selectedAnimal))
                        return false;

                    animalInventory.RemoveAnimal(selectedAnimal);

                    Debug.Log(
                        $"{selectedAnimal.animalName} placed into {enclosure.EnclosureName}!"
                    );

                    selectedAnimal = null;
                    return true;
                }
            }
        }

        // -------------------------
        // 2. WORLD / GROUND PLACEMENT
        // -------------------------
        if (selectedAnimal.animalData == null ||
            selectedAnimal.animalData.prefab == null)
        {
            Debug.LogWarning(
                "Selected animal has no AnimalData or prefab assigned for world placement."
            );

            return false;
        }

        Vector3 spawnPosition;
        if (WorldGrid.Instance != null)
        {
            Vector2Int cell = WorldGrid.Instance.WorldToCell(worldPosition);
            spawnPosition = WorldGrid.Instance.CellToWorldCenter(cell);
        }
        else
        {
            spawnPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);
        }

        GameObject spawnedAnimal = Instantiate(
            selectedAnimal.animalData.prefab,
            spawnPosition,
            Quaternion.identity
        );

        Animal animalComponent = spawnedAnimal.GetComponent<Animal>();
        if (animalComponent == null)
        {
            Destroy(spawnedAnimal);
            Debug.LogWarning("Animal prefab has no Animal component; the animal remains in net storage.");
            return false;
        }

        animalComponent.LoadFromInstance(selectedAnimal);

        animalInventory.RemoveAnimal(selectedAnimal);

        Debug.Log($"Placed {selectedAnimal.animalName} on the ground!");

        selectedAnimal = null;

        return true;
    }
}
