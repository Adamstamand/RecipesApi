using RecipesCore.Entities;

namespace RecipesCore.RepositoryContracts;

public interface IRecipesRepository
{
    void AddRecipe(Recipe recipe);
    Task<IEnumerable<Recipe>> AllRecipes();
    Task<Recipe> SpecificRecipe(int id);
    void UpdateRecipe();
    void RemoveRecipe();
}