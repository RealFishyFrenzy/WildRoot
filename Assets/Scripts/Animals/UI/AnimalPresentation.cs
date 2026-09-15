using System.Globalization;

// Read-only formatting shared by stored-animal UI; never edits or clones records.
public static class AnimalPresentation
{
    public static string Number(float value) => float.IsNaN(value) || float.IsInfinity(value)
        ? "Unknown" : value.ToString("0.#", CultureInfo.InvariantCulture);

    public static string Compact(AnimalInstance animal)
    {
        if (animal == null) return "Animal unavailable";
        string maximum = animal.animalData != null ? Number(animal.animalData.baseMaxHunger) : "?";
        return $"{(string.IsNullOrWhiteSpace(animal.animalName) ? "Unnamed" : animal.animalName)}\n" +
            $"{animal.SpeciesName}\nFood: {Number(animal.foodLevel)} / {maximum}\nSex: {animal.sex}";
    }

    public static string Details(AnimalInstance animal)
    {
        if (animal == null) return "Animal unavailable";
        AnimalData data = animal.animalData;
        SpeciesData species = data != null ? data.species : null;
        return Compact(animal) + $"\nHealth: {Number(animal.health)} / {(data != null ? Number(data.baseMaxHealth) : "?")}" +
            $"\nAge: {Number(animal.ageYears)} years" +
            (species != null && !string.IsNullOrWhiteSpace(species.scientificName) ? $"\nScientific name: {species.scientificName}" : "");
    }
}
