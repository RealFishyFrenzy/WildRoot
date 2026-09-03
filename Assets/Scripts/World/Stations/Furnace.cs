using UnityEngine;

public class Furnace : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private Hotbar hotbar;

    [Header("Recipes")]
    [SerializeField] private CraftingRecipe[] recipes;

    private void Awake()
    {
        if (inventory == null)
            inventory = FindAnyObjectByType<PlayerInventory>();

        if (hotbar == null)
            hotbar = FindAnyObjectByType<Hotbar>();
    }

    public void Interact()
    {
        if (inventory == null || hotbar == null)
            return;

        ItemData heldItem = hotbar.SelectedItem;

        if (heldItem == null)
        {
            Debug.Log("Hold a processable item first.");
            return;
        }

        foreach (CraftingRecipe recipe in recipes)
        {
            if (recipe == null ||
                recipe.ingredients == null ||
                recipe.ingredients.Length == 0)
            {
                continue;
            }

            // Ingredient 0 determines what item must be held.
            CraftingIngredient mainIngredient =
                recipe.ingredients[0];

            if (mainIngredient.item != heldItem)
                continue;

            if (!CraftingSystem.TryCraft(inventory, recipe))
            {
                Debug.Log(
                    $"Missing ingredients for {recipe.recipeName}."
                );

                return;
            }

            HotbarUI hotbarUI =
                FindAnyObjectByType<HotbarUI>();

            if (hotbarUI != null)
                hotbarUI.Refresh();

            Debug.Log(
                $"Crafted {recipe.outputAmount} " +
                $"{recipe.outputItem.itemName}!"
            );

            return;
        }

        Debug.Log(
            $"{heldItem.itemName} can't be processed here."
        );
    }
}