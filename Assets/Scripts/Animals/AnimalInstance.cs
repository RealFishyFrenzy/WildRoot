using System;
using UnityEngine;

[Serializable]
public class AnimalInstance
{
    public string animalName;
    public string speciesName;

    public float foodLevel;
    public float maxFoodLevel;

    public GameObject animalPrefab;

    public AnimalInstance(
        string animalName,
        string speciesName,
        float foodLevel,
        float maxFoodLevel,
        GameObject animalPrefab)
    {
        this.animalName = animalName;
        this.speciesName = speciesName;
        this.foodLevel = foodLevel;
        this.maxFoodLevel = maxFoodLevel;
        this.animalPrefab = animalPrefab;
    }
}