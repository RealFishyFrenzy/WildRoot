# Inventory and crafting regression checks

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet run --project Tests/InventoryLogic/InventoryLogic.csproj
```

The harness compiles the actual inventory and crafting sources with minimal Unity
stand-ins. It tests item quantities, capacity, transaction failure, duplicate
ingredients, and change notifications. It does not test Unity lifecycle, physics
pickup callbacks, or scene UI. Generated files go under Temp.

The 0.0.5 pass expands this to 33 scenarios, including whole-slot swaps/moves,
invalid moves, unchanged slot identities, same-item stacks without merging,
non-stackable upgrade-shaped recipe transactions, and bounded/expiring notification
state and explicit notification events. Upgrade fixture costs are test-only;
no improved-tool gameplay values have been approved or added.

Tool tests also compile the actual ToolItem, ToolInstance, ToolTarget, and
TerrainToolSystem sources. They check Bronze asset configuration and starting
references, independent durability, disabled depletion, successful/failed use,
definition immutability, state-preserving swaps/transactions, and fresh crafted
instances. The Bronze asset path checks now use their current Bronze-prefixed
filenames, and depletion verifies the current exact-instance removal behavior.
ToolTarget health subtraction is real; Unity object lookup, coroutine
execution, destruction, terrain lookup and drop creation are stand-ins. These
checks do not prove physics picking, resource-drop prefabs, hotbar input, or UI
rendering. Inspector-assigned fields may produce CS0649 warnings in this harness.

Hand-crafting checks exercise the real presentation-independent menu state:
explicit catalog/context filtering, whole-inventory and duplicate ingredient
counts, event subscription lifecycle, craft-time revalidation, atomic output-space
failure, fresh tool copies, wear/swap/exact break removal, malformed recipes and
empty catalogs. Recipe fixture values are test-only; no hand recipe assets are
added. Runtime tab widgets, UI input blocking and Furnace interaction still need
Unity Play Mode checks.

Resource-node checks cover inclusive hit-range endpoints, one-time initialization,
legacy baseline fallback, reversed ranges, minimum power rejection with feedback
and notification, no durability cost on rejection, excess-power scaling relative
to requirements, extreme integer values and one-time destruction. Feedback is
observed through a test override; physical shake/overlap dispatch needs Play Mode.
