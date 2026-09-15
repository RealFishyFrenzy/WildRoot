using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFishPool",
    menuName = "WildRoot/Fishing/Fish Pool"
)]
public class FishPool : ScriptableObject
{
    [System.Serializable]
    public class FishEntry
    {
        public AnimalData animal;

        [Min(0f)]
        public float weight = 1f;
    }

    [SerializeField] private List<FishEntry> fish = new();

    public AnimalData GetRandomFish()
    {
        if (fish == null || fish.Count == 0)
            return null;

        float totalWeight = 0f;

        foreach (FishEntry entry in fish)
        {
            if (entry.animal != null && entry.weight > 0f)
                totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float roll = Random.Range(0f, totalWeight);

        foreach (FishEntry entry in fish)
        {
            if (entry.animal == null || entry.weight <= 0f)
                continue;

            roll -= entry.weight;

            if (roll <= 0f)
                return entry.animal;
        }

        return null;
    }
}