using System.Collections.Generic;
using UnityEngine;

public class AnimalInventory : MonoBehaviour
{
    [SerializeField] private List<AnimalInstance> animals = new List<AnimalInstance>();

    public IReadOnlyList<AnimalInstance> Animals => animals;

    public void AddAnimal(AnimalInstance animal)
    {
        if (animal == null)
            return;

        animals.Add(animal);

        Debug.Log(
            $"Captured {animal.animalName} the {animal.speciesName}! " +
            $"Animals owned: {animals.Count}"
        );
    }

    public bool RemoveAnimal(AnimalInstance animal)
    {
        if (animal == null)
            return false;

        return animals.Remove(animal);
    }
}