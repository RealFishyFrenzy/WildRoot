using System;
using System.Reflection;
using UnityEngine;

static class Program
{
    static void Call(object target, string method) => target.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
    static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    static void Check(bool result, string message) { if (!result) throw new Exception(message); }
    static void Near(float a, float b) => Check(Math.Abs(a - b) < 0.001, $"Expected {b}, got {a}");
    static GameClock clock;
    static void AdvanceHour()
    {
        Time.deltaTime = 62.5f; // 1500 seconds / 24 hours: actual default GameClock conversion.
        Call(clock, "Update");
    }
    static Animal Spawn(AnimalInstance instance)
    {
        var animal = new Animal();
        Call(animal, "OnEnable");
        Call(animal, "Start"); // Must not subscribe twice.
        animal.LoadFromInstance(instance);
        return animal;
    }

    static void Main()
    {
        clock = new GameClock();
        MonoBehaviour.Objects[typeof(GameClock)] = clock;
        Call(clock, "Awake");
        var data = new AnimalData();
        var initial = new AnimalInstance(data, "Bob", 50.12345f, 87, 2, AnimalSex.Male);
        var animal = Spawn(initial);
        AdvanceHour();
        Check(animal.FoodLevel == initial.foodLevel, "Zero rate changed food");
        data.foodLossPerGameHour = 6; // Test fixture only; no gameplay asset changed.
        AdvanceHour();
        Near(animal.FoodLevel, initial.foodLevel - 6);
        Time.deltaTime = 0;
        float before = animal.FoodLevel;
        Call(clock, "Update");
        Check(animal.FoodLevel == before, "Paused clock changed hunger");

        var net = new AnimalInventory();
        var catcher = new CatchableAnimal();
        Set(catcher, "animal", animal);
        Set(catcher, "animalInventory", net);
        Call(catcher, "Awake");
        Check(catcher.UseTool(ToolType.Net, 1), "Catch failed");
        Check(!catcher.UseTool(ToolType.Net, 1), "Repeated capture accepted");
        var stored = net.Animals[0];
        Check(stored.foodLevel == before, "Catch did not preserve exact food");
        AdvanceHour();
        Check(stored.foodLevel == before && animal.FoodLevel == before, "Net/deferred world object progressed");

        var tank = new AnimalEnclosure();
        Check(tank.AddAnimal(stored) && net.RemoveAnimal(stored), "Net to tank failed");
        AdvanceHour();
        Check(stored.foodLevel == before, "Tank progressed");
        Check(tank.RemoveAnimal(stored), "Tank removal failed");
        net.AddAnimal(stored);
        animal = Spawn(stored);
        net.RemoveAnimal(stored);
        Check(animal.FoodLevel == before, "World load changed food");
        AdvanceHour();
        Near(animal.FoodLevel, before - 6);
        Set(catcher, "animal", animal);
        Check(catcher.UseTool(ToolType.Net, 1), "Recapture failed");
        Check(net.Animals[0].foodLevel == animal.FoodLevel, "Recapture changed food");
        Check(net.Animals[0].health == 87 && net.Animals[0].ageYears == 2 && net.Animals[0].sex == AnimalSex.Male,
            "Unrelated stats changed");

        Check(data.CalculateFoodAfterElapsedHours(1, 100) == 0, "Lower clamp");
        Check(data.CalculateFoodAfterElapsedHours(200, 0) == 100, "Upper clamp");
        data.foodLossPerGameHour = -6;
        Check(data.CalculateFoodAfterElapsedHours(50, 1) == 50, "Negative rate feeds animal");
        data.foodLossPerGameHour = 6;
        clock = new GameClock();
        Set(clock, "startingHour", 23);
        Set(clock, "startingMinute", 59);
        MonoBehaviour.Objects[typeof(GameClock)] = clock;
        animal = Spawn(initial);
        AdvanceHour();
        Near(animal.FoodLevel, initial.foodLevel - 6);
        Check(clock.Day == 2, "Midnight test did not roll over");
        animal.enabled = false;

        var missing = new AnimalInstance(null, "Missing", 25, 100, 0, AnimalSex.Unknown);
        net.AddAnimal(missing);
        Check(missing.SpeciesName == "Unknown Species", "Missing data fallback");
        Check(initial.SpeciesName == "Unknown Species", "Missing species fallback");
        animal = Spawn(missing);
        AdvanceHour();
        Check(animal.FoodLevel == 25, "Missing data changed food");
        animal.enabled = false;
        RunAdditionalChecks();
        Console.WriteLine("PASS: zero rate, hourly rate, pause, single subscription, capture, frozen net/tank, exact transfers, recapture, unchanged stats, clamping, invalid rate, midnight, missing data/species.");
    }

    static void RunAdditionalChecks()
    {
        var limited = new AnimalInventory();
        Check(limited.Capacity == 5, "Basic net capacity must be five");
        int capacityEvents = 0; limited.Changed += () => capacityEvents++;
        for (int i = 0; i < 5; i++)
            Check(limited.AddAnimal(new AnimalInstance(null, "Stored " + i, 10 + i, 100, 0, AnimalSex.Unknown)), "Fill net");
        var extra = new AnimalInstance(null, "Extra", 22.123f, 99, 2, AnimalSex.Female);
        Check(!limited.AddAnimal(extra) && capacityEvents == 5 && !limited.HasSpace, "Full net admitted animal/notified");
        var sourceTank = new AnimalEnclosure(); sourceTank.AddAnimal(extra);
        Check(!limited.TryTakeFromEnclosure(sourceTank, extra) && sourceTank.Animals[0] == extra,
            "Full net removed animal from tank");
        var live = Spawn(new AnimalInstance(new AnimalData(), "Live", 44.123f, 88, 1, AnimalSex.Male));
        var fullCatcher = new CatchableAnimal();
        Set(fullCatcher, "animal", live); Set(fullCatcher, "animalInventory", limited); Call(fullCatcher, "Awake");
        Check(!fullCatcher.UseTool(ToolType.Net, 1) && live.enabled && live.FoodLevel == 44.123f,
            "Full capture disabled or changed world animal");
        live.enabled = false;
        Set(limited, "capacity", 7); // configuration fixture; not UI hardcoding
        Check(limited.Capacity == 7 && limited.TryTakeFromEnclosure(sourceTank, extra), "Configured capacity/transfer failed");
        Check(sourceTank.Animals.Count == 0 && limited.Contains(extra) && extra.foodLevel == 22.123f,
            "Successful tank transfer changed state");
        Check(!limited.AddAnimal(extra), "Duplicate record admitted");
        Set(limited, "capacity", 2);
        Check(limited.Count == 6 && !limited.HasSpace, "Lower capacity deleted existing animals");
        Check(AnimalPresentation.Compact(extra).Contains("Female") &&
            AnimalPresentation.Compact(extra).Contains("Unknown Species"), "Tooltip missing-data/sex formatting");
        Check(AnimalPresentation.Details(extra).Contains("Health:") && AnimalPresentation.Details(extra).Contains("Age:"),
            "Details not richer than tooltip");
        Check(AnimalPresentation.Number(float.NaN) == "Unknown" && extra.foodLevel == 22.123f,
            "Presentation corrupted record or invalid value");
        Console.WriteLine("PASS: capacity five/configurable, full catch/tank rejection, exact transfer, duplicates, over-capacity preservation and shared formatting.");

        // Menu list uses the same multi-animal storage and listens only to mutations.
        var listStorage = new AnimalInventory();
        var first = new AnimalInstance(null, "First", 12.345f, 80, 0, AnimalSex.Unknown);
        var second = new AnimalInstance(null, "Second", 67.891f, 90, 1, AnimalSex.Female);
        int changes = 0;
        Action changed = () => changes++;
        listStorage.Changed += changed;
        listStorage.AddAnimal(null);
        Check(!listStorage.Contains(null) && changes == 0, "Null list operation notified");
        listStorage.AddAnimal(first); listStorage.AddAnimal(second);
        Check(changes == 2 && listStorage.Animals.Count == 2, "Multi-animal list failed");
        Check(ReferenceEquals(listStorage.Animals[1], second) && listStorage.Contains(second), "Second animal identity lost");
        Check(listStorage.RemoveAnimal(second) && changes == 3, "Second animal removal failed");
        Check(!listStorage.RemoveAnimal(second) && changes == 3, "Failed removal notified");
        Check(listStorage.Animals[0] == first && first.foodLevel == 12.345f && second.foodLevel == 67.891f,
            "List mutation changed animal food/state");
        listStorage.Changed -= changed;
        listStorage.RemoveAnimal(first);
        Check(changes == 3, "Unsubscribed list listener invoked");
        Console.WriteLine("PASS: multi-animal storage, non-first identity/removal, exact food, mutation notifications and unsubscribe.");

        var data = new AnimalData { foodLossPerGameHour = 3 };
        var instance = new AnimalInstance(data, "Lifecycle", 80, 95, 4, AnimalSex.Female);
        var animal = Spawn(instance);
        animal.enabled = false;
        AdvanceHour();
        Check(animal.FoodLevel == 80, "Disabled animal progressed");
        animal.enabled = true;
        Check(animal.FoodLevel == 80, "Re-enable applied catch-up");
        AdvanceHour();
        Near(animal.FoodLevel, 77);
        animal.LoadFromInstance(instance);
        Check(animal.FoodLevel == 80, "Reload failed exact preservation");
        AdvanceHour();
        Near(animal.FoodLevel, 77);
        animal.LoadFromInstance(null);
        Near(animal.FoodLevel, 77);
        animal.enabled = false;

        // Different definitions must not share mutable individual food state.
        var slower = new AnimalData { foodLossPerGameHour = 1 };
        var slowAnimal = Spawn(new AnimalInstance(slower, "Slow", 50, 100, 0, AnimalSex.Unknown));
        animal = Spawn(instance);
        AdvanceHour();
        Near(slowAnimal.FoodLevel, 49);
        Near(animal.FoodLevel, 77);
        Check(data.foodLossPerGameHour == 3 && slower.foodLossPerGameHour == 1, "Data asset mutated");
        slowAnimal.enabled = false;
        animal.enabled = false;

        MonoBehaviour.Objects.Remove(typeof(GameClock));
        var noClock = Spawn(instance);
        Check(noClock.FoodLevel == 80, "Missing clock changed food");
        noClock.enabled = false;
        MonoBehaviour.Objects[typeof(GameClock)] = clock;
        noClock.enabled = true;
        AdvanceHour();
        Near(noClock.FoodLevel, 77);
        noClock.enabled = false;

        var net = new AnimalInventory();
        net.AddAnimal(null);
        Check(net.Animals.Count == 0 && !net.RemoveAnimal(null), "Null storage behavior");
        var tank = new AnimalEnclosure();
        Check(!tank.AddAnimal(null) && !tank.RemoveAnimal(null), "Null enclosure behavior");
        var invalid = new AnimalInstance(null, "Missing", 21.12345f, 88, 3, AnimalSex.Unknown);
        net.AddAnimal(invalid);
        Check(tank.AddAnimal(invalid) && net.RemoveAnimal(invalid), "Missing-data transfer failed");
        Check(tank.Animals[0].foodLevel == 21.12345f, "Missing-data transfer changed food");

        animal = new Animal();
        animal.LoadFromInstance(instance);
        animal.Feed(float.NaN);
        Check(animal.FoodLevel == 80, "NaN feeding corrupted food");
        animal.Feed(float.PositiveInfinity);
        Check(animal.FoodLevel == 80, "Infinite feeding changed food");
        data.baseMaxHunger = float.NaN;
        animal.Feed(1);
        Check(animal.FoodLevel == 80, "Invalid maximum corrupted food");
        data.baseMaxHunger = 100;
        animal.Feed(5);
        Check(animal.FoodLevel == 85, "Valid feeding changed behavior");

        foreach (float rate in new[] { float.NaN, float.PositiveInfinity, -1f, 0f })
        {
            data.foodLossPerGameHour = rate;
            Check(data.CalculateFoodAfterElapsedHours(40, 1) == 40, "Invalid/zero rate changed food");
        }
        Console.WriteLine("PASS: disable/re-enable, no catch-up, reload, independent rates, missing clock, null transfers, invalid feeding, invalid rates.");
    }
}
