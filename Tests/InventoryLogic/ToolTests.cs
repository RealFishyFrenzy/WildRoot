using System;
using System.IO;
using System.Reflection;
using UnityEngine;

static partial class Program
{
    // All breakable values are synthetic test fixtures, not content/balance.
    static ToolItem Tool(bool unbreakable = false, int maximum = 10, int cost = 2)
    {
        var tool = new ToolItem { toolType = ToolType.Pickaxe, stackable = false };
        Set(tool, "unbreakable", unbreakable);
        Set(tool, "maximumDurability", maximum);
        Set(tool, "durabilityCostPerUse", cost);
        return tool;
    }

    static void Set(object target, string field, object value) =>
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    static void RunToolTests()
    {
        Run("Node inclusive initialization, invalid ranges and legacy baseline", () =>
        {
            UnityEngine.Random.value = 0;
            Check(Node(1, 4, 5).RemainingHitPoints == 4);
            UnityEngine.Random.value = 1;
            Check(Node(1, 4, 5).RemainingHitPoints == 5);
            Check(Node(1, 5, 2).RemainingHitPoints == 5);
            Check(Node(1, 0, 0).RemainingHitPoints == 3);
            Check(Node(1, int.MaxValue, int.MaxValue).RemainingHitPoints == int.MaxValue);
            var node = Node(1, 4, 5);
            UnityEngine.Random.value = 0;
            node.InitializeAgain();
            Check(node.RemainingHitPoints == 5);
        });
        Run("Underpowered strikes shake and notify without damage or durability loss", () =>
        {
            var node = Node(2, 4, 4);
            var tool = Tool(); var inventory = Inventory(2); inventory.AddItem(tool, 2);
            var state = inventory.GetSlot(0).ToolState;
            string message = null;
            Action<string, string> listener = (text, key) => message = text;
            GameplayNotifications.Posted += listener;
            try
            {
                Check(!inventory.RecordToolUse(state, node.UseTool(ToolType.Pickaxe, 1)));
                Check(message == "Need a stronger Pickaxe" && node.Feedback == 1);
                Check(node.RemainingHitPoints == 4 && state.CurrentDurability == 10);
                Check(!node.UseTool(ToolType.Axe, 10) && node.Feedback == 1);
                Check(inventory.RecordToolUse(state, node.UseTool(ToolType.Pickaxe, 2)));
                Check(node.RemainingHitPoints == 3 && state.CurrentDurability == 8);
                Check(inventory.GetSlot(1).ToolState.CurrentDurability == 10);
            }
            finally { GameplayNotifications.Posted -= listener; }
        });
        Run("Excess power scales relative to requirement and destruction happens once", () =>
        {
            foreach (int baseline in new[] { 4, 5 })
                foreach (int required in new[] { 1, 3 })
                    for (int excess = 0; excess < 3; excess++)
                    {
                        var node = Node(required, baseline, baseline);
                        int hits = 0;
                        while (node.UseTool(ToolType.Pickaxe, required + excess)) hits++;
                        Check(hits == (excess == 0 ? baseline : excess == 1 ? 3 : 2));
                        Check(node.Destructions == 1 && node.RemainingHitPoints == 0);
                    }
            var extreme = Node(1, int.MaxValue, int.MaxValue);
            Check(!extreme.UseTool(ToolType.Pickaxe, int.MinValue));
            Check(extreme.UseTool(ToolType.Pickaxe, int.MaxValue) && extreme.Destructions == 1);
        });
        Run("Bronze starter asset contracts and GUID-based starting inventory", () =>
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !Directory.Exists(Path.Combine(root.FullName, "Assets", "Data"))) root = root.Parent;
            Check(root != null);
            string scene = File.ReadAllText(Path.Combine(root.FullName, "Assets/Scenes/SampleScene.unity"));
            foreach (string name in new[] { "Axe", "Pickaxe", "Shovel" })
            {
                string path = Path.Combine(root.FullName, "Assets/Data/Items/Tools/Bronze" + name + ".asset");
                string data = File.ReadAllText(path);
                foreach (string field in new[] { "itemName: Bronze " + name, "power: 1", "tier: 1", "unbreakable: 1", "maximumDurability: 0", "stackable: 0" })
                    Check(data.Contains("  " + field + "\n") || data.Contains("  " + field + "\r\n"));
                string guid = File.ReadAllText(path + ".meta").Split("guid: ")[1].Split('\n')[0].Trim();
                Check(scene.Contains(guid));
            }
            string net = File.ReadAllText(Path.Combine(root.FullName, "Assets/Data/Items/Tools/Net.asset"));
            Check(net.Contains("itemName: Net") && !net.Contains("tier:") && !net.Contains("maximumDurability:"));
        });
        Run("Unbreakable is independent of tier and never consumes durability", () =>
        {
            var tool = Tool(true, 0, int.MaxValue);
            Set(tool, "tier", ToolTier.Bronze);
            var state = new ToolInstance(tool);
            Check(tool.Power == 1 && tool.Tier == ToolTier.Bronze && state.Unbreakable);
            for (int i = 0; i < 100; i++) Check(!state.RecordUse(true));
            Check(state.CurrentDurability == 0 && state.CanUse && !state.IsBroken);
            Set(tool, "tier", ToolTier.Titanium);
            Check(state.CanUse && !state.RecordUse(true));
            Set(tool, "power", 7);
            Check(tool.Power == 7 && tool.Tier == ToolTier.Titanium);
        });
        Run("Independent copies, nonstackable enforcement, exact ownership and swap", () =>
        {
            var tool = Tool(); tool.stackable = true; // malformed definition must not merge tools
            var inventory = Inventory(20);
            Check(inventory.AddItem(tool, 2));
            var a = inventory.GetSlot(0).ToolState;
            var b = inventory.GetSlot(1).ToolState;
            Check(a != b && inventory.GetSlot(0).quantity == 1 && inventory.GetSlot(1).quantity == 1);
            int events = 0; inventory.Changed += () => events++;
            Check(!inventory.RecordToolUse(a, false) && events == 0);
            Check(!inventory.RecordToolUse(new ToolInstance(tool), true));
            Check(inventory.RecordToolUse(a, true) && events == 1);
            Check(a.CurrentDurability == 8 && b.CurrentDurability == 10 && tool.MaximumDurability == 10);
            Check(inventory.TryMoveOrSwap(0, 9));
            Check(inventory.GetSlot(9).ToolState == a && inventory.GetSlot(0).ToolState == null);
            Check(inventory.TryMoveOrSwap(9, 1));
            Check(inventory.GetSlot(1).ToolState == a && inventory.GetSlot(9).ToolState == b);
            Check(inventory.AddItem(Item(), 2)); // successful transaction preserves state identity
            Check(inventory.GetSlot(1).ToolState == a && a.CurrentDurability == 8);
            inventory.GetSlot(1).Clear();
            Check(!inventory.RecordToolUse(a, true) && b.CurrentDurability == 10);
        });
        Run("Depletion removes the exact broken instance without deleting another tool", () =>
        {
            var inventory = Inventory(2); var tool = Tool(false, 3, int.MaxValue);
            inventory.AddItem(tool, 2);
            var a = inventory.GetSlot(0).ToolState;
            Check(inventory.RecordToolUse(a, true));
            Check(a.CurrentDurability == 0 && a.IsBroken && !a.CanUse);
            Check(!inventory.RecordToolUse(a, true) && inventory.GetQuantity(tool) == 1);
            Check(inventory.GetSlot(0).IsEmpty);
            Check(inventory.GetSlot(1).ToolState.CurrentDurability == 3);
            Check(!new ToolInstance(Tool(false, -1, 1)).CanUse);
            Check(!new ToolInstance(Tool(false, 10, -1)).CanUse);
            Check(!new ToolInstance(null).CanUse);
        });
        Run("Crafting independent tools preserves worn tools and initializes new instances", () =>
        {
            var tool = Tool(); var bars = Item(); var inventory = Inventory(3);
            inventory.AddItem(tool); inventory.AddItem(bars, 2);
            var existing = inventory.GetSlot(0).ToolState;
            inventory.RecordToolUse(existing, true);
            var recipe = Recipe(tool, 1, Ingredient(bars, 2));
            Check(CraftingSystem.CanCraft(inventory, recipe) && existing.CurrentDurability == 8);
            Check(CraftingSystem.TryCraft(inventory, recipe));
            Check(inventory.GetSlot(0).ToolState == existing && existing.CurrentDurability == 8);
            Check(inventory.GetSlot(1).ToolState != existing && inventory.GetSlot(1).ToolState.CurrentDurability == 10);
            Check(!inventory.AddItem(tool, 2));
            Check(existing.CurrentDurability == 8 && inventory.GetQuantity(tool) == 2);
        });
        Run("Legacy serialized slots initialize once and clear discards instance", () =>
        {
            var tool = Tool(); var slot = new InventorySlot { item = tool, quantity = 1 };
            var original = slot.ToolState;
            original.RecordUse(true);
            Check(slot.ToolState == original && slot.ToolState.CurrentDurability == 8);
            slot.Clear(); Check(slot.ToolState == null);
            slot.item = tool; slot.quantity = 1;
            Check(slot.ToolState != original && slot.ToolState.CurrentDurability == 10);
        });
        Run("Actual target success drives wear; empty/wrong tool does not", () =>
        {
            var tool = Tool(); var inventory = Inventory(1); inventory.AddItem(tool);
            var state = inventory.GetSlot(0).ToolState;
            var target = new GatheringTarget();
            var go = new GameObject { component = target };
            Check(!inventory.RecordToolUse(state, tool.Use(null)));
            Check(!inventory.RecordToolUse(state, tool.Use(new GameObject())));
            target.Configure(ToolType.Axe, 3);
            Check(!inventory.RecordToolUse(state, tool.Use(go)) && state.CurrentDurability == 10);
            target.Configure(ToolType.Pickaxe, 3);
            Check(inventory.RecordToolUse(state, tool.Use(go)) && target.Health == 2);
            Check(state.CurrentDurability == 8);
            var bronze = Tool(true, 0, 0); bronze.toolType = ToolType.Pickaxe;
            Check(bronze.Use(go) && target.Health == 1);
            Check(bronze.Use(go) && target.Destroyed);
            tool.toolType = ToolType.Net;
            Check(!inventory.RecordToolUse(state, true) && state.CurrentDurability == 8);
        });
        Run("Terrain succeeds only when a valid shovel operation spawns a drop", () =>
        {
            var system = new TerrainToolSystem(); var tool = Tool(); tool.toolType = ToolType.Shovel;
            var state = new ToolInstance(tool);
            TerrainManager.Instance = null;
            Check(!system.UseTool(tool, default));
            TerrainManager.Instance = new TerrainManager { terrain = TerrainType.Sand };
            Check(!state.RecordUse(system.UseTool(tool, default)) && state.CurrentDurability == 10);
            Set(system, "sandItem", Item()); Set(system, "dirtItem", Item());
            Set(system, "worldItemDropPrefab", new WorldItemDrop());
            WorldItemDrop.Spawned = 0;
            Check(state.RecordUse(system.UseTool(tool, default)) && WorldItemDrop.Spawned == 1);
            TerrainManager.Instance.terrain = TerrainType.Grass;
            Check(state.RecordUse(system.UseTool(tool, default)) && WorldItemDrop.Spawned == 2);
            tool.toolType = ToolType.Axe;
            Check(!state.RecordUse(system.UseTool(tool, default)) && state.CurrentDurability == 6);
        });
    }

    class GatheringTarget : ToolTarget
    {
        public double Health => RemainingHitPoints;
        public bool Destroyed;
        public void Configure(ToolType type, int hitPoints) { requiredTool = type; health = hitPoints; }
        protected override void OnDestroyed() { Destroyed = true; }
    }

    static NodeTarget Node(int power, int minimum, int maximum)
    {
        var node = new NodeTarget();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(ToolTarget).GetField("minimumToolPower", flags).SetValue(node, power);
        typeof(ToolTarget).GetField("minimumHitsToBreak", flags).SetValue(node, minimum);
        typeof(ToolTarget).GetField("maximumHitsToBreak", flags).SetValue(node, maximum);
        node.InitializeAgain();
        return node;
    }

    class NodeTarget : ToolTarget
    {
        public int Feedback;
        public int Destructions;
        public void InitializeAgain() { requiredTool = ToolType.Pickaxe; base.Awake(); }
        protected override void PlayHitFeedback() { Feedback++; }
        protected override void OnDestroyed() { Destructions++; }
    }
}
