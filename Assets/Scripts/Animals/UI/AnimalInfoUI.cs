using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimalInfoUI : MonoBehaviour
{
    public static AnimalInfoUI Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text animalNameText;
    [SerializeField] private TMP_Text speciesText;
    [SerializeField] private TMP_Text hungerText;
    [SerializeField] private TMP_Text healthText;

    private Animal currentAnimal;
    private GameClock gameClock;
    private AnimalInstance storedAnimal;
    private AnimalInventory storedOwner;
    private GameObject storedPanel;
    private TMP_Text storedDetails;

    public bool IsOpen => (panel != null && panel.activeInHierarchy) ||
        (storedPanel != null && storedPanel.activeInHierarchy);
    public Animal CurrentAnimal => currentAnimal;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        gameClock = FindAnyObjectByType<GameClock>();
        if (gameClock != null)
            gameClock.MinuteChanged += OnGameMinuteChanged;
    }

    private void OnDestroy()
    {
        CloseStored();
        if (storedPanel != null) Destroy(storedPanel);
        if (gameClock != null)
            gameClock.MinuteChanged -= OnGameMinuteChanged;

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        if (storedAnimal != null)
        {
            if (InventoryUI.Instance == null || !InventoryUI.Instance.IsOpen)
                CloseStored();
            else storedDetails.text = AnimalPresentation.Details(storedAnimal);
            return;
        }

        // If the referenced animal was destroyed, disabled, or removed while open
        if (currentAnimal == null || !currentAnimal.gameObject.activeInHierarchy || currentAnimal.AnimalData == null)
        {
            Close();
            return;
        }

        // Keep values in sync with live animal state
        RefreshStats();
    }

    private void OnGameMinuteChanged(GameTime time)
    {
        if (IsOpen)
            RefreshStats();
    }

    public void Open(Animal animal)
    {
        if (animal == null || animal.AnimalData == null)
            return;

        Close();

        currentAnimal = animal;

        if (panel != null)
            panel.SetActive(true);

        RefreshAll();
    }

    public void Close()
    {
        CloseStored();
        currentAnimal = null;

        if (panel != null)
            panel.SetActive(false);
    }

    public void Refresh()
    {
        RefreshAll();
    }

    public void OpenStored(AnimalInstance animal, AnimalInventory storage)
    {
        if (animal == null || storage == null || !storage.Contains(animal) ||
            InventoryUI.Instance == null || !InventoryUI.Instance.IsOpen)
            return;
        Close();
        if (storedPanel == null)
        {
            Canvas canvas = panel != null ? panel.GetComponentInParent<Canvas>() : FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            RectTransform root = WildRootUITheme.CreatePanel(canvas.transform, "Stored Animal Info (Runtime)", WildRootUITheme.PanelFrame);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0, 0);
            root.sizeDelta = new Vector2(460, 420);
            storedPanel = root.gameObject;

            WildRootUITheme.CreateHeaderRibbon(root, "Details Header", "CREATURE PROFILE", "Animal Inspection");
            RectTransform header = root.Find("Details Header") as RectTransform;
            if (header != null)
            {
                header.anchorMin = new Vector2(0.04f, 1f);
                header.anchorMax = new Vector2(0.82f, 1f);
                header.pivot = new Vector2(0f, 1f);
                header.anchoredPosition = new Vector2(0, -12);
                header.sizeDelta = new Vector2(0, 48);
            }

            Button closeBtn = WildRootUITheme.CreateButton(root, "Close Details Button", "✕", 18f, WildRootUITheme.ButtonClose, WildRootUITheme.ButtonClose);
            RectTransform closeRect = (RectTransform)closeBtn.transform;
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-16, -12);
            closeRect.sizeDelta = new Vector2(48, 48);
            closeBtn.onClick.AddListener(CloseStored);

            RectTransform inner = WildRootUITheme.CreatePanel(root, "Details Inner", WildRootUITheme.PanelInner);
            inner.anchorMin = Vector2.zero;
            inner.anchorMax = Vector2.one;
            inner.offsetMin = new Vector2(16, 64);
            inner.offsetMax = new Vector2(-16, -68);

            storedDetails = WildRootUITheme.CreateText(inner, "Details Text", "", 15f, FontStyles.Normal, TextAlignmentOptions.Left, WildRootUITheme.ColorTextBody);
            WildRootUITheme.FitStretch(storedDetails.rectTransform, 16, 12, 16, 12);

            Button bottomClose = WildRootUITheme.CreateButton(root, "Bottom Close", "Back to Inventory", 15f, WildRootUITheme.ButtonNormal, WildRootUITheme.ButtonHover);
            RectTransform bcRect = (RectTransform)bottomClose.transform;
            bcRect.anchorMin = new Vector2(0.2f, 0f);
            bcRect.anchorMax = new Vector2(0.8f, 0f);
            bcRect.pivot = new Vector2(0.5f, 0f);
            bcRect.anchoredPosition = new Vector2(0, 14);
            bcRect.sizeDelta = new Vector2(0, 40);
            bottomClose.onClick.AddListener(CloseStored);
        }
        storedAnimal = animal;
        storedOwner = storage;
        storedOwner.Changed += OnStorageChanged;
        storedDetails.text = AnimalPresentation.Details(animal);
        storedPanel.SetActive(true);
        storedPanel.transform.SetAsLastSibling();
    }

    private void OnStorageChanged()
    {
        if (storedOwner == null || !storedOwner.Contains(storedAnimal)) CloseStored();
        else if (storedDetails != null) storedDetails.text = AnimalPresentation.Details(storedAnimal);
    }

    public void CloseStored()
    {
        if (storedOwner != null) storedOwner.Changed -= OnStorageChanged;
        storedOwner = null;
        storedAnimal = null;
        if (storedPanel != null) storedPanel.SetActive(false);
    }

    private void OnDisable() => Close();

    private void RefreshAll()
    {
        if (storedAnimal != null)
        {
            storedDetails.text = AnimalPresentation.Details(storedAnimal);
            return;
        }
        if (currentAnimal == null || currentAnimal.AnimalData == null)
        {
            Close();
            return;
        }

        if (animalNameText != null)
        {
            animalNameText.text = !string.IsNullOrEmpty(currentAnimal.AnimalName)
                ? currentAnimal.AnimalName
                : "Unnamed";
        }

        if (speciesText != null)
        {
            string speciesName = currentAnimal.Species != null && !string.IsNullOrEmpty(currentAnimal.Species.commonName)
                ? currentAnimal.Species.commonName
                : (currentAnimal.AnimalData != null ? currentAnimal.AnimalData.name : "Unknown Species");

            speciesText.text = $"Species: {speciesName}";
        }

        RefreshStats();
    }

    private void RefreshStats()
    {
        if (currentAnimal == null || currentAnimal.AnimalData == null)
            return;

        if (hungerText != null)
        {
            float currentFood = currentAnimal.FoodLevel;
            float maxFood = currentAnimal.AnimalData.baseMaxHunger;
            hungerText.text = $"Hunger: {Mathf.RoundToInt(currentFood)} / {Mathf.RoundToInt(maxFood)}";
        }

        if (healthText != null)
        {
            float currentHealth = currentAnimal.Health;
            float maxHealth = currentAnimal.AnimalData.baseMaxHealth;
            healthText.text = $"Health: {Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        }
    }
}
