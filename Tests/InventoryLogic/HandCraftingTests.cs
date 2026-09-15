using System;

static partial class Program
{
    static CraftingRecipe HandRecipe(ItemData output, int amount, params CraftingIngredient[] ingredients)
    {
        var recipe = Recipe(output, amount, ingredients);
        recipe.context = RecipeContext.HandCrafting;
        return recipe;
    }

    static void RunHandCraftingTests()
    {
        Run("Hand catalog excludes stations/nulls/duplicates without changing station crafting", () =>
        {
            var raw = Item(); var output = Item(); var inventory = Inventory(2);
            var station = Recipe(output, 1, Ingredient(raw, 1));
            var hand = HandRecipe(output, 1, Ingredient(raw, 1));
            using var menu = new HandCraftingMenuState(inventory, new[] { station, null, hand, hand });
            Check(station.context == RecipeContext.StationProcessing);
            Check(menu.Recipes.Count == 1 && menu.Recipes[0] == hand);
            Check(!menu.Select(station) && !menu.Craft());
            inventory.AddItem(raw);
            Check(CraftingSystem.TryCraft(inventory, station)); // existing station backend remains context-neutral
            Check(!menu.Select(HandRecipe(output, 1, Ingredient(raw, 1))));
        });
        Run("Whole-inventory counts, duplicate requirements and event-driven availability", () =>
        {
            var raw = Item(); var output = Item(); var inventory = Inventory(20);
            var recipe = HandRecipe(output, 2, Ingredient(raw, 1), Ingredient(raw, 2));
            using var menu = new HandCraftingMenuState(inventory, new[] { recipe });
            menu.Observe(); menu.Observe(); // no duplicate inventory subscription
            menu.Select(recipe);
            int events = 0; menu.Changed += () => events++;
            Check(!menu.CanCraft);
            inventory.AddItem(raw, 3);
            Check(events == 1 && menu.CanCraft);
            inventory.TryMoveOrSwap(0, 19);
            Check(menu.GetIngredients().Count == 1 && menu.GetIngredients()[0].Required == 3);
            Check(menu.GetIngredients()[0].Owned == 3 && menu.ResultItem == output && menu.ResultQuantity == 2);
            Check(menu.Craft() && inventory.GetQuantity(raw) == 0 && inventory.GetQuantity(output) == 2);
            Check(!menu.CanCraft && menu.GetIngredients()[0].Owned == 0);
            menu.Select(null); Check(menu.Selected == null && !menu.CanCraft && menu.GetIngredients().Count == 0);
            menu.Dispose(); events = 0;
            inventory.AddItem(raw); Check(events == 0);
            menu.Observe(); Check(events == 1);
        });
        Run("Craft press revalidates stale availability and changed classification", () =>
        {
            var raw = Item(); var inventory = Inventory(2);
            var recipe = HandRecipe(Item(), 1, Ingredient(raw, 2));
            using var menu = new HandCraftingMenuState(inventory, new[] { recipe });
            inventory.AddItem(raw, 2); menu.Select(recipe);
            Check(menu.CanCraft);
            inventory.RemoveItem(raw);
            Check(!menu.Craft() && inventory.GetQuantity(raw) == 1);
            inventory.AddItem(raw); recipe.context = RecipeContext.StationProcessing;
            Check(!menu.Craft() && inventory.GetQuantity(raw) == 2);
        });
        Run("Hand output-space failure is atomic; freed ingredient slots are reusable", () =>
        {
            var raw = Item(); var output = Tool(); var inventory = Inventory(1);
            var recipe = HandRecipe(output, 1, Ingredient(raw, 2));
            using var menu = new HandCraftingMenuState(inventory, new[] { recipe });
            inventory.AddItem(raw, 3); menu.Select(recipe);
            Check(!menu.CanCraft && !menu.Craft() && inventory.GetQuantity(raw) == 3);
            inventory.RemoveItem(raw);
            Check(menu.CanCraft && menu.Craft() && inventory.GetQuantity(output) == 1);
            Check(inventory.GetSlot(0).ToolState.CurrentDurability == output.MaximumDurability);
        });
        Run("Hand-crafted copies keep independent wear, swaps and exact break removal", () =>
        {
            var raw = Item(); var tool = Tool(false, 4, 2); var inventory = Inventory(20);
            var recipe = HandRecipe(tool, 1, Ingredient(raw, 1));
            using var menu = new HandCraftingMenuState(inventory, new[] { recipe });
            inventory.AddItem(raw, 2); menu.Select(recipe);
            Check(menu.Craft()); var a = inventory.GetSlot(1).ToolState;
            Check(menu.Craft()); var b = inventory.GetSlot(0).ToolState;
            Check(a != b && a.CurrentDurability == 4 && b.CurrentDurability == 4);
            Check(inventory.RecordToolUse(a, true));
            Check(a.CurrentDurability == 2 && b.CurrentDurability == 4);
            Check(inventory.TryMoveOrSwap(1, 9) && inventory.GetSlot(9).ToolState == a);
            Check(inventory.RecordToolUse(a, true) && inventory.GetSlot(9).IsEmpty);
            Check(inventory.GetSlot(0).ToolState == b && b.CurrentDurability == 4);
            Check(tool.MaximumDurability == 4 && inventory.GetQuantity(tool) == 1);
        });
        Run("Empty catalog, absent inventory and malformed recipes are safe", () =>
        {
            using var empty = new HandCraftingMenuState(null, null);
            empty.Observe(); Check(empty.Recipes.Count == 0 && !empty.Craft());
            var raw = Item(); var recipe = HandRecipe(Item(), 1, Ingredient(raw, int.MaxValue), Ingredient(raw, 1));
            using var menu = new HandCraftingMenuState(Inventory(2), new[] { recipe });
            menu.Select(recipe);
            Check(menu.GetIngredients().Count == 0 && !menu.CanCraft && !menu.Craft());
        });
    }
}
