using UnityEngine;

public static class CraftingSystem
{
    public static bool CanCraft(
        PlayerInventory inventory,
        CraftingRecipe recipe)
    {
        if (inventory == null || recipe == null)
            return false;

        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            if (ingredient.item == null)
                return false;

            if (!inventory.HasItem(ingredient.item, ingredient.amount))
                return false;
        }

        return true;
    }

    public static bool TryCraft(
        PlayerInventory inventory,
        CraftingRecipe recipe)
    {
        if (!CanCraft(inventory, recipe))
            return false;

        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(
                ingredient.item,
                ingredient.amount
            );
        }

        inventory.AddItem(
            recipe.outputItem,
            recipe.outputAmount
        );

        return true;
    }
}