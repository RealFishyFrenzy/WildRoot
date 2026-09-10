using System;

[Serializable]
public class AnimalInstance
{
    public AnimalData animalData;

    public string animalName;

    public float foodLevel;
    public float health;
    public float ageYears;

    public AnimalSex sex;

    public string SpeciesName => animalData != null && animalData.species != null &&
        !string.IsNullOrEmpty(animalData.species.commonName)
        ? animalData.species.commonName : "Unknown Species";

    public AnimalInstance(
        AnimalData animalData,
        string animalName,
        float foodLevel,
        float health,
        float ageYears,
        AnimalSex sex)
    {
        this.animalData = animalData;
        this.animalName = animalName;
        this.foodLevel = foodLevel;
        this.health = health;
        this.ageYears = ageYears;
        this.sex = sex;
    }
}
