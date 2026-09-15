using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCraftingRecipe",
    menuName = "Crafting/Recipe"
)]
public class CraftingRecipe : ScriptableObject
{
    [Header("Recipe")]
    public string recipeName;
    // Zero preserves existing station-only recipes without asset migration.
    public RecipeContext context = RecipeContext.StationProcessing;

    [Header("Ingredients")]
    public CraftingIngredient[] ingredients;

    [Header("Output")]
    public ItemData outputItem;
    public int outputAmount = 1;
}
