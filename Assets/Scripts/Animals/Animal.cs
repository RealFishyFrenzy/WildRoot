using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string animalName = "Unnamed";
    [SerializeField] private string speciesName = "Unknown Species";

    [Header("Needs")]
    [SerializeField] private float foodLevel = 50f;
    [SerializeField] private float maxFoodLevel = 100f;

    [SerializeField] private GameObject animalPrefab;

    public string AnimalName => animalName;
    public string SpeciesName => speciesName;

    public void Feed(float amount)
    {
        foodLevel += amount;
        foodLevel = Mathf.Clamp(foodLevel, 0f, maxFoodLevel);

        Debug.Log($"{animalName} ate! Food: {foodLevel}/{maxFoodLevel}");
    }

    public AnimalInstance GetAnimalInstance()
    {
        return new AnimalInstance(
            animalName,
            speciesName,
            foodLevel,
            maxFoodLevel,
            animalPrefab
        );
    }

    public void LoadFromInstance(AnimalInstance instance)
    {
        animalName = instance.animalName;
        speciesName = instance.speciesName;
        foodLevel = instance.foodLevel;
        maxFoodLevel = instance.maxFoodLevel;
        animalPrefab = instance.animalPrefab;
    }
}