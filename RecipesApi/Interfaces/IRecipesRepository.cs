using RecipesApi.Models;

namespace RecipesApi.Interfaces;

public interface IRecipesRepository
{
    void AddRecipe(Recipe recipe);
    Task<IEnumerable<Recipe>> AllRecipes();
    void UpdateRecipe();
    void RemoveRecipe();
}