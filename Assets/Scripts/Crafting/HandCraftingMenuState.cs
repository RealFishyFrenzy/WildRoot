using System;
using System.Collections.Generic;

// Presentation-independent hand-crafting selection and live inventory view.
// The supplied catalog is the availability boundary; future discovery can filter it.
public sealed class HandCraftingMenuState : IDisposable
{
    private readonly PlayerInventory inventory;
    private readonly List<CraftingRecipe> recipes = new List<CraftingRecipe>();
    private bool observing;
    public event Action Changed;
    public IReadOnlyList<CraftingRecipe> Recipes { get; }
    public CraftingRecipe Selected { get; private set; }
    public ItemData ResultItem => Selected != null ? Selected.outputItem : null;
    public int ResultQuantity => Selected != null ? Selected.outputAmount : 0;
    public bool CanCraft => IsAvailable(Selected) && CraftingSystem.CanCraft(inventory, Selected);

    public HandCraftingMenuState(PlayerInventory inventory, IEnumerable<CraftingRecipe> catalog)
    {
        this.inventory = inventory;
        Recipes = recipes.AsReadOnly();
        if (catalog != null)
            foreach (CraftingRecipe recipe in catalog)
                if (recipe != null && recipe.context == RecipeContext.HandCrafting && !recipes.Contains(recipe))
                    recipes.Add(recipe);
    }

    public void Observe()
    {
        if (!observing && inventory != null) inventory.Changed += Notify;
        observing = true;
        Notify();
    }

    public void Dispose()
    {
        if (observing && inventory != null) inventory.Changed -= Notify;
        observing = false;
    }

    public bool Select(CraftingRecipe recipe)
    {
        if (recipe != null && !IsAvailable(recipe)) return false;
        Selected = recipe;
        Notify();
        return true;
    }

    public bool IsAvailable(CraftingRecipe recipe) => recipe != null &&
        recipe.context == RecipeContext.HandCrafting && recipes.Contains(recipe);

    public bool CanCraftRecipe(CraftingRecipe recipe) => IsAvailable(recipe) &&
        CraftingSystem.CanCraft(inventory, recipe);

    public IReadOnlyList<HandCraftingIngredientState> GetIngredients()
    {
        var result = new List<HandCraftingIngredientState>();
        if (!CraftingSystem.TryGetIngredients(Selected, out var requirements)) return result.AsReadOnly();
        foreach (var requirement in requirements)
            result.Add(new HandCraftingIngredientState(requirement.Key, requirement.Value,
                inventory != null ? inventory.GetQuantity(requirement.Key) : 0));
        return result.AsReadOnly();
    }

    public bool Craft()
    {
        // Never trust a previously displayed CanCraft value or button state.
        if (!IsAvailable(Selected)) return false;
        bool crafted = CraftingSystem.TryCraft(inventory, Selected);
        if (!crafted || !observing) Notify();
        return crafted;
    }

    private void Notify() => Changed?.Invoke();
}

public sealed class HandCraftingIngredientState
{
    public ItemData Item { get; }
    public int Required { get; }
    public int Owned { get; }
    public HandCraftingIngredientState(ItemData item, int required, int owned)
    { Item = item; Required = required; Owned = owned; }
}
