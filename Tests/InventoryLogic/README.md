# Inventory and crafting regression checks

Run from the repository root with the .NET 10 SDK:

```powershell
dotnet run --project Tests/InventoryLogic/InventoryLogic.csproj
```

The harness compiles the actual inventory and crafting sources with minimal Unity
stand-ins. It tests item quantities, capacity, transaction failure, duplicate
ingredients, and change notifications. It does not test Unity lifecycle, physics
pickup callbacks, or scene UI. Generated files go under Temp.
