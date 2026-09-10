using UnityEngine;

[CreateAssetMenu(fileName = "NewFood", menuName = "Items/Food")]
public class FoodItem : ItemData
{
    [SerializeField] private float foodValue = 10f;

    public override bool Use(GameObject target)
    {
        if (target != null && target.TryGetComponent(out Animal animal) && animal.AnimalData != null)
        {
            animal.Feed(foodValue);
            return true;
        }

        Debug.Log($"{itemName} can't be fed to that.");
        return false;
    }
}
