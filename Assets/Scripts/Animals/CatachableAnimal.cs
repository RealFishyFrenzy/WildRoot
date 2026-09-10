using UnityEngine;

public class CatchableAnimal : ToolTarget
{
    [SerializeField] private Animal animal;
    [SerializeField] private AnimalInventory animalInventory;

    protected override void Awake()
    {
        base.Awake();

        requiredTool = ToolType.Net;

        if (animal == null)
            animal = GetComponent<Animal>();

        if (animalInventory == null)
            animalInventory = FindAnyObjectByType<AnimalInventory>();
    }

    public override bool UseTool(ToolType toolType, int power)
    {
        if (toolType != requiredTool)
            return false;

        if (animal == null)
            animal = GetComponent<Animal>();

        if (animalInventory == null)
            animalInventory = FindAnyObjectByType<AnimalInventory>();

        if (animal == null || animalInventory == null)
            return false;

        if (!animal.enabled)
            return false;

        // Stop clock notifications before taking the snapshot (Destroy is deferred).
        animal.enabled = false;

        AnimalInstance capturedAnimal = animal.GetAnimalInstance();

        animalInventory.AddAnimal(capturedAnimal);

        Debug.Log($"Caught {capturedAnimal.animalName}!");

        Destroy(gameObject);

        return true;
    }
}
