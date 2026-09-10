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

    public bool IsOpen => panel != null && panel.activeSelf;
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
        if (gameClock != null)
            gameClock.MinuteChanged -= OnGameMinuteChanged;

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsOpen)
            return;

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

        currentAnimal = animal;

        if (panel != null)
            panel.SetActive(true);

        RefreshAll();
    }

    public void Close()
    {
        currentAnimal = null;

        if (panel != null)
            panel.SetActive(false);
    }

    public void Refresh()
    {
        RefreshAll();
    }

    private void RefreshAll()
    {
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
