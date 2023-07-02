using RecipesApi.Models;

namespace RecipesApi.Interfaces;

public interface IRecipesRepository
{
    void AddRecipe(Recipe recipe);
    Task<IEnumerable<Recipe>> AllRecipes();
    Task<Recipe> SpecificRecipe(int id);
    void UpdateRecipe();
    void RemoveRecipe();
}