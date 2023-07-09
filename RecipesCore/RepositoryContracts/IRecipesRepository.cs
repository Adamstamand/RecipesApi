using RecipesCore.DTOs;
using RecipesCore.Entities;

namespace RecipesCore.RepositoryContracts;

public interface IRecipesRepository
{
    Task<Recipe> AddRecipe(UserRecipe addRecipe);
    Task<IEnumerable<Recipe>> AllRecipes();
    Task<Recipe> SpecificRecipe(int id);
    void UpdateRecipe();
    void RemoveRecipe();
}