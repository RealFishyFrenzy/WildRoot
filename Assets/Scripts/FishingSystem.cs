using UnityEngine;

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private GameObject bobberPrefab;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private AnimalInventory animalInventory;

    private GameObject activeBobber;
    private FishingBobber activeFishingBobber;
    private FishPool activeFishPool;

    public bool IsFishing => activeBobber != null;
    public FishPool ActiveFishPool => activeFishPool;

    private void Awake()
    {
        Instance = this;
    }

    public bool TryUseFishingRod(Vector3 worldPosition)
    {
        // If we're already fishing, this click reels the line in.
        if (IsFishing)
        {
            Debug.Log(
                $"Reeling in. Bobber component: {activeFishingBobber != null}, " +
                $"HasBite: {activeFishingBobber != null && activeFishingBobber.HasBite}"
            );

            // Only catch a fish if the player reels in during the bite window.
            if (activeFishingBobber != null &&
                activeFishingBobber.HasBite)
            {
                TryCatchFish();
            }

            StopFishing();
            return true;
        }

        // Terrain system must exist.
        if (TerrainManager.Instance == null)
        {
            Debug.LogError("FISHING FAILED: TerrainManager.Instance is null.");
            return false;
        }

        // Rod can only be cast onto water.
        TerrainType terrain =
            TerrainManager.Instance.GetTerrainType(worldPosition);

        if (terrain != TerrainType.Water)
            return false;

        // Water must also belong to a FishingZone.
        FishingZone zone = FindFishingZone(worldPosition);

        if (zone == null)
        {
            Debug.LogWarning(
                "FISHING FAILED: Water tile is not inside a FishingZone."
            );
            return false;
        }

        if (zone.FishPool == null)
        {
            Debug.LogWarning(
                $"FISHING FAILED: FishingZone '{zone.name}' has no FishPool assigned."
            );
            return false;
        }

        activeFishPool = zone.FishPool;

        Debug.Log($"Fishing in pool: {activeFishPool.name}");

        StartFishing(worldPosition);

        return true;
    }

    private void StartFishing(Vector3 worldPosition)
    {
        if (bobberPrefab == null)
        {
            Debug.LogError("FISHING FAILED: Bobber Prefab is not assigned.");
            activeFishPool = null;
            return;
        }

        Vector3 spawnPosition = worldPosition;

        // Snap the bobber to the center of the clicked world-grid cell.
        if (WorldGrid.Instance != null)
        {
            Vector2Int cell =
                WorldGrid.Instance.WorldToCell(worldPosition);

            spawnPosition =
                WorldGrid.Instance.CellToWorldCenter(cell);
        }

        activeBobber =
            Instantiate(
                bobberPrefab,
                spawnPosition,
                Quaternion.identity
            );

        // Get the bite-state component from the spawned bobber.
        activeFishingBobber =
            activeBobber.GetComponent<FishingBobber>();

        if (activeFishingBobber == null)
        {
            Debug.LogError(
                "FISHING WARNING: Spawned Bobber does not have a FishingBobber component."
            );
        }

        // Player can still use the fishing rod,
        // but normal movement is disabled while the line is cast.
        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(true);
        }
        else
        {
            Debug.LogError(
                "FISHING WARNING: Player Movement reference is not assigned."
            );
        }
    }

    public void StopFishing()
    {
        if (activeBobber != null)
            Destroy(activeBobber);

        activeBobber = null;
        activeFishingBobber = null;
        activeFishPool = null;

        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);
    }

    private FishingZone FindFishingZone(Vector3 worldPosition)
    {
        FishingZone[] zones =
            FindObjectsByType<FishingZone>();

        foreach (FishingZone zone in zones)
        {
            if (zone.Contains(worldPosition))
                return zone;
        }

        return null;
    }

    private void TryCatchFish()
    {
        Debug.Log("TryCatchFish called.");

        if (activeFishPool == null)
        {
            Debug.LogError(
                "CATCH FAILED: activeFishPool is null."
            );
            return;
        }

        if (animalInventory == null)
        {
            Debug.LogError(
                "CATCH FAILED: Animal Inventory reference is not assigned."
            );
            return;
        }

        if (!animalInventory.HasSpace)
        {
            Debug.LogWarning(
                $"CATCH FAILED: Animal inventory is full. " +
                $"{animalInventory.Count}/{animalInventory.Capacity}"
            );
            return;
        }

        // Ask the current pond/river/etc. what animal was caught.
        AnimalData caughtAnimal =
            activeFishPool.GetRandomFish();

        if (caughtAnimal == null)
        {
            Debug.LogError(
                $"CATCH FAILED: FishPool '{activeFishPool.name}' returned no AnimalData."
            );
            return;
        }

        if (caughtAnimal.species == null)
        {
            Debug.LogError(
                $"CATCH FAILED: AnimalData '{caughtAnimal.name}' has no SpeciesData."
            );
            return;
        }

        // Wild-caught animals currently have a 50/50 sex roll.
        AnimalSex sex =
            Random.value < 0.5f
                ? AnimalSex.Male
                : AnimalSex.Female;

        // Create the individual living animal.
        AnimalInstance caughtInstance =
            new AnimalInstance(
                caughtAnimal,
                caughtAnimal.species.commonName,
                caughtAnimal.baseMaxHunger,
                caughtAnimal.baseMaxHealth,
                0f,
                sex
            );

        // Fishing captures directly into the player's animal inventory.
        bool added =
            animalInventory.AddAnimal(caughtInstance);

        Debug.Log(
            $"Catch result: {caughtAnimal.species.commonName}, " +
            $"Sex: {sex}, Added to animal inventory: {added}"
        );
    }
}