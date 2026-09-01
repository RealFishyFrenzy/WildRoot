using TMPro;
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

    private void Refresh()
    {
        if (animalInventory == null)
            return;

        if (animalInventory.Animals.Count == 0)
        {
            animalButton.gameObject.SetActive(false);
            return;
        }

        animalButton.gameObject.SetActive(true);

        AnimalInstance animal = animalInventory.Animals[0];

        animalButtonText.text =
            $"{animal.animalName}\n{animal.speciesName}";
    }

    private void SelectAnimal()
    {
        if (animalInventory.Animals.Count == 0)
            return;

        selectedAnimal = animalInventory.Animals[0];

        Debug.Log(
            $"Selected {selectedAnimal.animalName} for placement."
        );

        panel.SetActive(false);
    }

    public bool TryPlaceSelectedAnimal(Vector3 worldPosition)
    {
        if (selectedAnimal == null)
            return false;

        // Check what the player clicked on.
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
        {
            Debug.Log("Can't place animal here.");
            return false;
        }

        // See if that object is a valid animal placement zone.
        AnimalPlacementZone zone =
            hit.GetComponent<AnimalPlacementZone>();

        if (zone == null)
        {
            Debug.Log("No valid animal placement zone here.");
            return false;
        }

        // Bob can currently only go in water or a tank.
        if (zone.ZoneType != PlacementZoneType.Tank &&
            zone.ZoneType != PlacementZoneType.Water)
        {
            Debug.Log("This animal can't be placed here.");
            return false;
        }

        // -------------------------
        // TANK PLACEMENT
        // -------------------------
        // Tanks store the AnimalInstance instead of spawning
        // an animal GameObject into the world.
        if (zone.ZoneType == PlacementZoneType.Tank)
        {
            AnimalEnclosure enclosure =
                hit.GetComponent<AnimalEnclosure>();

            if (enclosure == null)
            {
                Debug.LogWarning(
                    "Tank placement zone has no AnimalEnclosure."
                );

                return false;
            }

            // Try adding Bob to this specific tank.
            if (!enclosure.AddAnimal(selectedAnimal))
                return false;

            // Bob is now inside the tank, so remove him from the net.
            animalInventory.RemoveAnimal(selectedAnimal);

            Debug.Log(
                $"{selectedAnimal.animalName} placed into {enclosure.EnclosureName}!"
            );

            selectedAnimal = null;

            Refresh();

            return true;
        }

        // -------------------------
        // WORLD / WATER PLACEMENT
        // -------------------------
        // If Bob isn't being placed into a tank, he needs
        // a prefab so he can physically exist in the world.
        if (selectedAnimal.animalPrefab == null)
        {
            Debug.LogWarning(
                "Selected animal has no prefab assigned for world placement."
            );

            return false;
        }

        // Convert the mouse position to a grid cell.
        Vector2Int cell =
            WorldGrid.Instance.WorldToCell(worldPosition);

        // Get the center of that tile.
        Vector3 snappedPosition =
            WorldGrid.Instance.CellToWorldCenter(cell);

        // Spawn Bob at the center of the valid tile.
        GameObject spawnedAnimal =
            Instantiate(
                selectedAnimal.animalPrefab,
                snappedPosition,
                Quaternion.identity
            );

        Animal animalComponent =
            spawnedAnimal.GetComponent<Animal>();

        if (animalComponent != null)
        {
            animalComponent.LoadFromInstance(selectedAnimal);
        }

        // Only remove Bob from the net AFTER placement succeeds.
        animalInventory.RemoveAnimal(selectedAnimal);

        Debug.Log($"Placed {selectedAnimal.animalName}!");

        selectedAnimal = null;

        Refresh();

        return true;
    }
}