using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NetUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button animalButton;
    [SerializeField] private TMP_Text animalButtonText;

    [Header("Animal Storage")]
    [SerializeField] private AnimalInventory animalInventory;

    private AnimalInstance selectedAnimal;

    public bool HasAnimalSelected => selectedAnimal != null;

    private void Start()
    {
        if (animalInventory == null)
            animalInventory = FindAnyObjectByType<AnimalInventory>();

        panel.SetActive(false);

        animalButton.onClick.AddListener(SelectAnimal);
    }

    public void Toggle()
    {
        bool newState = !panel.activeSelf;

        panel.SetActive(newState);

        if (newState)
            Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void CancelPlacement()
    {
        selectedAnimal = null;
        Close();
    }

    private void Refresh()
    {
        if (animalInventory == null)
            animalInventory = FindAnyObjectByType<AnimalInventory>();

        if (animalInventory == null)
            return;

        if (animalInventory.Animals.Count == 0)
        {
            animalButton.gameObject.SetActive(false);
            return;
        }

        animalButton.gameObject.SetActive(true);

        AnimalInstance animal = animalInventory.Animals[0];
        if (animal == null)
        {
            animalButton.gameObject.SetActive(false);
            return;
        }

        animalButtonText.text =
            $"{animal.animalName}\n{animal.SpeciesName}";
    }

    private void SelectAnimal()
    {
        if (animalInventory == null)
            animalInventory = FindAnyObjectByType<AnimalInventory>();

        if (animalInventory == null || animalInventory.Animals.Count == 0)
            return;

        selectedAnimal = animalInventory.Animals[0];

        if (selectedAnimal == null)
            return;

        Debug.Log(
            $"Selected {selectedAnimal.animalName} for placement."
        );

        panel.SetActive(false);
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
                    Refresh();
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

        Refresh();

        return true;
    }
}
