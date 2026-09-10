using UnityEngine;

[CreateAssetMenu(
    fileName = "NewAnimal",
    menuName = "Animals/Animal"
)]
public class AnimalData : ScriptableObject
{
    [Header("Species")]
    public SpeciesData species;

    [Header("World")]
    public GameObject prefab;

    [Header("Base Stats")]
    public float baseMaxHunger = 100f;
    public float baseMaxHealth = 100f;

    [Header("Needs")]
    [Min(0f)] public float foodLossPerGameHour = 0f;

    public float CalculateFoodAfterElapsedHours(float currentFood, double elapsedHours)
    {
        if (float.IsNaN(currentFood) || float.IsInfinity(currentFood) ||
            float.IsNaN(baseMaxHunger) || float.IsInfinity(baseMaxHunger) || baseMaxHunger < 0f)
            return currentFood;

        double food = currentFood;
        if (elapsedHours > 0 && !double.IsNaN(elapsedHours) && !double.IsInfinity(elapsedHours) &&
            foodLossPerGameHour > 0 && !float.IsNaN(foodLossPerGameHour) && !float.IsInfinity(foodLossPerGameHour))
            food -= foodLossPerGameHour * elapsedHours;

        return (float)System.Math.Max(0, System.Math.Min(baseMaxHunger, food));
    }
}
