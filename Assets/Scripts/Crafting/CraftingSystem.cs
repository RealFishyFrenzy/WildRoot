using System.Collections.Generic;

public static class CraftingSystem
{
    public static bool CanCraft(PlayerInventory inventory, CraftingRecipe recipe)
    {
        return inventory != null && TryGetIngredients(recipe, out var ingredients) &&
            inventory.CanExchange(ingredients, recipe.outputItem, recipe.outputAmount);
    }

    public static bool TryCraft(PlayerInventory inventory, CraftingRecipe recipe)
    {
        return inventory != null && TryGetIngredients(recipe, out var ingredients) &&
            inventory.TryExchange(ingredients, recipe.outputItem, recipe.outputAmount);
    }

    private static bool TryGetIngredients(
        CraftingRecipe recipe, out Dictionary<ItemData, int> ingredients)
    {
        ingredients = new Dictionary<ItemData, int>();
        if (recipe == null || recipe.outputItem == null || recipe.outputAmount <= 0 ||
            recipe.ingredients == null || recipe.ingredients.Length == 0)
            return false;

        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            if (ingredient == null || ingredient.item == null || ingredient.amount <= 0)
                return false;

            ingredients.TryGetValue(ingredient.item, out int existing);
            if (ingredient.amount > int.MaxValue - existing)
                return false;

            ingredients[ingredient.item] = existing + ingredient.amount;
        }
        return true;
    }
}
