using System.Collections.Generic;
using UnityEngine;

public class AnimalInventory : MonoBehaviour
{
    [SerializeField] private List<AnimalInstance> animals = new List<AnimalInstance>();
    [SerializeField, Min(0)] private int capacity = 5;
    public int Capacity => System.Math.Max(0, capacity);
    public int Count => animals.Count;
    public bool HasSpace => Count < Capacity;

    public IReadOnlyList<AnimalInstance> Animals => animals;
    public event System.Action Changed;

    public bool Contains(AnimalInstance animal) => animal != null && animals.Contains(animal);

    public bool AddAnimal(AnimalInstance animal)
    {
        if (animal == null || !HasSpace || Contains(animal))
            return false;

        animals.Add(animal);
        Changed?.Invoke();

        Debug.Log(
            $"Captured {animal.animalName} the {animal.SpeciesName}! " +
            $"Animals owned: {animals.Count}"
        );
        return true;
    }

    public bool TryTakeFromEnclosure(AnimalEnclosure enclosure, AnimalInstance animal)
    {
        if (enclosure == null || animal == null || !HasSpace || Contains(animal))
            return false;
        if (!enclosure.RemoveAnimal(animal))
            return false;
        return AddAnimal(animal);
    }

    public bool RemoveAnimal(AnimalInstance animal)
    {
        if (animal == null)
            return false;

        bool removed = animals.Remove(animal);
        if (removed)
            Changed?.Invoke();
        return removed;
    }
}
