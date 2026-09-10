using System.Collections.Generic;
using UnityEngine;

public class AnimalEnclosure : MonoBehaviour, IInteractable
{
    [Header("Enclosure")]
    [SerializeField] private string enclosureName = "Unnamed Enclosure";
    [SerializeField] private int animalCapacity = 10;

    [Header("Animals")]
    [SerializeField]
    private List<AnimalInstance> animals =
        new List<AnimalInstance>();

    public string EnclosureName => enclosureName;
    public IReadOnlyList<AnimalInstance> Animals => animals;

    public int AnimalCapacity => animalCapacity;

    public bool AddAnimal(AnimalInstance animal)
    {
        if (animal == null)
            return false;

        if (animals.Count >= animalCapacity)
        {
            Debug.Log($"{enclosureName} is full.");
            return false;
        }

        animals.Add(animal);

        Debug.Log(
            $"{animal.animalName} added to {enclosureName}. " +
            $"Animals: {animals.Count}/{animalCapacity}"
        );

        return true;
    }

    public void Interact()
    {
        EnclosureUI.Instance.Open(this);
    }

    public bool RemoveAnimal(AnimalInstance animal)
    {
        if (animal == null)
            return false;

        return animals.Remove(animal);
    }
}