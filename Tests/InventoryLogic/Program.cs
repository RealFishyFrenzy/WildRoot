using System;
using System.Reflection;

static class Program
{
    private static int passed;

    static void Main()
    {
        Run("Stack filling and overflow rejection", () =>
        {
            var item = Item();
            var inventory = Inventory(2);
            Check(inventory.AddItem(item, 8));
            Check(inventory.AddItem(item, 5));
            Check(inventory.GetSlot(0).quantity == 10 && inventory.GetSlot(1).quantity == 3);
            Check(!inventory.AddItem(item, 8));
            Check(inventory.GetQuantity(item) == 13);
        });
        Run("Partial pickups retain the unaccepted remainder", () =>
        {
            var item = Item();
            var inventory = Inventory(1);
            inventory.AddItem(item, 8);
            int remaining = 5;
            remaining -= inventory.AddItemPartial(item, remaining);
            Check(remaining == 3 && inventory.GetQuantity(item) == 10);
            Check(inventory.AddItemPartial(item, remaining) == 0);
            inventory.RemoveItem(item, 3);
            remaining -= inventory.AddItemPartial(item, remaining);
            Check(remaining == 0 && inventory.GetQuantity(item) == 10);
        });
        Run("Nonstackable items each occupy one slot", () =>
        {
            var item = Item(false);
            var inventory = Inventory(2);
            Check(!inventory.AddItem(item, 3) && inventory.GetQuantity(item) == 0);
            Check(inventory.AddItemPartial(item, 3) == 2);
        });
        Run("Insufficient removal leaves inventory untouched", () =>
        {
            var item = Item();
            var inventory = Inventory(2);
            inventory.AddItem(item, 15);
            Check(!inventory.RemoveItem(item, 16));
            Check(inventory.GetQuantity(item) == 15);
            Check(inventory.RemoveItem(item, 7));
            Check(inventory.GetSlot(1).IsEmpty && inventory.GetSlot(0).quantity == 8);
        });
        Run("Crafting without output space consumes nothing", () =>
        {
            var raw = Item();
            var output = Item();
            var inventory = Inventory(1);
            inventory.AddItem(raw, 10);
            var recipe = Recipe(output, 1, Ingredient(raw, 5));
            Check(!CraftingSystem.CanCraft(inventory, recipe));
            Check(!CraftingSystem.TryCraft(inventory, recipe));
            Check(inventory.GetQuantity(raw) == 10 && inventory.GetQuantity(output) == 0);
        });
        Run("Crafting can use a slot freed by ingredients", () =>
        {
            var raw = Item();
            var output = Item();
            var inventory = Inventory(1);
            inventory.AddItem(raw, 5);
            var recipe = Recipe(output, 1, Ingredient(raw, 5));
            var slotReference = inventory.GetSlot(0);
            int events = 0;
            inventory.Changed += () => events++;
            Check(CraftingSystem.CanCraft(inventory, recipe));
            Check(events == 0 && inventory.GetQuantity(raw) == 5);
            Check(CraftingSystem.TryCraft(inventory, recipe));
            Check(events == 1 && inventory.GetQuantity(raw) == 0 && inventory.GetQuantity(output) == 1);
            Check(ReferenceEquals(slotReference, inventory.GetSlot(0)));
        });
        Run("Duplicate ingredients require their combined quantity", () =>
        {
            var raw = Item();
            var output = Item();
            var inventory = Inventory(2);
            inventory.AddItem(raw, 5);
            var recipe = Recipe(output, 1, Ingredient(raw, 3), Ingredient(raw, 3));
            Check(!CraftingSystem.TryCraft(inventory, recipe));
            Check(inventory.GetQuantity(raw) == 5);
            inventory.AddItem(raw);
            Check(CraftingSystem.TryCraft(inventory, recipe));
            Check(inventory.GetQuantity(raw) == 0 && inventory.GetQuantity(output) == 1);
        });
        Run("Oversized output rolls back the complete craft", () =>
        {
            var raw = Item();
            var output = Item();
            var inventory = Inventory(1);
            inventory.AddItem(raw, 5);
            Check(!CraftingSystem.TryCraft(inventory, Recipe(output, 11, Ingredient(raw, 5))));
            Check(inventory.GetQuantity(raw) == 5);
        });
        Run("Output may be the same item as an ingredient", () =>
        {
            var item = Item();
            var inventory = Inventory(1);
            inventory.AddItem(item, 10);
            Check(CraftingSystem.TryCraft(inventory, Recipe(item, 2, Ingredient(item, 5))));
            Check(inventory.GetQuantity(item) == 7);
        });
        Run("Invalid recipes and overflowing ingredient totals are rejected", () =>
        {
            var item = Item();
            var inventory = Inventory(1);
            inventory.AddItem(item, 5);
            Check(!CraftingSystem.TryCraft(inventory, null));
            Check(!CraftingSystem.TryCraft(inventory, Recipe(null, 1, Ingredient(item, 1))));
            Check(!CraftingSystem.TryCraft(inventory, Recipe(item, 0, Ingredient(item, 1))));
            Check(!CraftingSystem.TryCraft(inventory, Recipe(item, 1)));
            Check(!CraftingSystem.TryCraft(inventory, Recipe(item, 1, new CraftingIngredient[] { null })));
            Check(!CraftingSystem.TryCraft(inventory, Recipe(item, 1, Ingredient(item, -1))));
            Check(!CraftingSystem.TryCraft(inventory, Recipe(item, 1,
                Ingredient(item, int.MaxValue), Ingredient(item, 1))));
            Check(inventory.GetQuantity(item) == 5);
        });
        Run("Only successful changes notify listeners", () =>
        {
            var item = Item();
            var inventory = Inventory(1);
            int events = 0;
            inventory.Changed += () => events++;
            Check(!inventory.AddItem(null));
            Check(!inventory.AddItem(item, 0));
            Check(!inventory.AddItem(item, -1));
            Check(!inventory.HasItem(null));
            Check(inventory.GetQuantity(null) == 0);
            Check(!inventory.RemoveItem(item));
            Check(events == 0);
            Check(inventory.AddItem(item, 10));
            Check(events == 1);
            Check(inventory.AddItemPartial(item, 1) == 0 && events == 1);
            Check(inventory.RemoveItem(item) && events == 2);
            item.maxStack = 0;
            Check(!inventory.AddItem(item) && inventory.AddItemPartial(item, 1) == 0);
            Check(events == 2);
        });
        Console.WriteLine($"Passed {passed} inventory/crafting regression scenarios.");
    }

    static PlayerInventory Inventory(int capacity)
    {
        var inventory = new PlayerInventory();
        typeof(PlayerInventory).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(inventory, new InventorySlot[capacity]);
        typeof(PlayerInventory).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(inventory, null);
        return inventory;
    }

    static ItemData Item(bool stackable = true) => new ItemData { stackable = stackable, maxStack = 10 };
    static CraftingIngredient Ingredient(ItemData item, int amount) =>
        new CraftingIngredient { item = item, amount = amount };
    static CraftingRecipe Recipe(ItemData output, int amount, params CraftingIngredient[] ingredients) =>
        new CraftingRecipe { outputItem = output, outputAmount = amount, ingredients = ingredients };
    static void Check(bool condition)
    {
        if (!condition) throw new Exception("Assertion failed.");
    }
    static void Run(string name, Action test)
    {
        test();
        passed++;
        Console.WriteLine("PASS: " + name);
    }
}
