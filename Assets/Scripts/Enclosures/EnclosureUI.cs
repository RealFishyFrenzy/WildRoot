using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnclosureUI : MonoBehaviour
{
    public static EnclosureUI Instance { get; private set; }

    [Header("Main UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text enclosureNameText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private Button closeButton;

    [Header("Animal List")]
    [SerializeField] private Transform animalList;
    [SerializeField] private GameObject animalEntryTemplate;

    [Header("Animal Storage")]
    [SerializeField] private AnimalInventory animalInventory;

    private AnimalEnclosure currentEnclosure;

    private void Awake()
    {
        Instance = this;

        if (animalInventory == null)
        {
            animalInventory =
                FindAnyObjectByType<AnimalInventory>();
        }
    }

    private void Start()
    {
        panel.SetActive(false);

        animalEntryTemplate.SetActive(false);

        closeButton.onClick.AddListener(Close);
    }

    public void Open(AnimalEnclosure enclosure)
    {
        if (enclosure == null)
            return;

        currentEnclosure = enclosure;

        panel.SetActive(true);

        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);

        currentEnclosure = null;
    }

    private void Refresh()
    {
        if (currentEnclosure == null)
            return;

        enclosureNameText.text =
            currentEnclosure.EnclosureName;

        capacityText.text =
            $"Animals {currentEnclosure.Animals.Count}/{currentEnclosure.AnimalCapacity}";

        ClearAnimalList();

        foreach (AnimalInstance animal in currentEnclosure.Animals)
        {
            if (animal == null)
                continue;

            GameObject entry =
                Instantiate(animalEntryTemplate, animalList);

            entry.SetActive(true);

            Transform nameTransform = entry.transform.Find("AnimalName");
            TMP_Text entryText = nameTransform != null ? nameTransform.GetComponent<TMP_Text>() : entry.GetComponentInChildren<TMP_Text>();

            if (entryText != null)
            {
                entryText.text =
                    $"{animal.animalName} ({animal.SpeciesName})";
            }

            Transform buttonTransform = entry.transform.Find("RemoveButton");
            Button takeButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : entry.GetComponentInChildren<Button>();

            if (takeButton != null)
            {
                AnimalInstance capturedAnimal = animal;

                takeButton.onClick.AddListener(() =>
                {
                    TakeAnimal(capturedAnimal);
                });
            }
        }
    }

    private void TakeAnimal(AnimalInstance animal)
    {
        if (animalInventory == null)
        {
            animalInventory = FindAnyObjectByType<AnimalInventory>();
        }

        if (animal == null ||
            currentEnclosure == null ||
            animalInventory == null)
        {
            return;
        }

        bool removed =
            currentEnclosure.RemoveAnimal(animal);

        if (!removed)
        {
            Debug.LogWarning(
                $"Could not remove {animal.animalName} from enclosure."
            );

            return;
        }

        animalInventory.AddAnimal(animal);

        Debug.Log(
            $"Took {animal.animalName} from {currentEnclosure.EnclosureName}."
        );

        Refresh();
    }

    private void ClearAnimalList()
    {
        foreach (Transform child in animalList)
        {
            if (child.gameObject == animalEntryTemplate)
                continue;

            Destroy(child.gameObject);
        }
    }
}
