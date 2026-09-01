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

    private AnimalEnclosure currentEnclosure;

    private void Awake()
    {
        Instance = this;
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
            // Create a new UI entry for this animal.
            GameObject entry =
                Instantiate(animalEntryTemplate, animalList);

            entry.SetActive(true);

            // Set the animal's name and species.
            TMP_Text entryText =
                entry.GetComponentInChildren<TMP_Text>();

            if (entryText != null)
            {
                entryText.text =
                    $"{animal.animalName}\n{animal.speciesName}";
            }

            // Make the animal entry clickable.
            Button entryButton =
                entry.GetComponent<Button>();

            if (entryButton != null)
            {
                AnimalInstance capturedAnimal = animal;

                entryButton.onClick.AddListener(() =>
                {
                    Debug.Log(
                        $"Selected {capturedAnimal.animalName} - " +
                        $"{capturedAnimal.speciesName}"
                    );
                });
            }
        }
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