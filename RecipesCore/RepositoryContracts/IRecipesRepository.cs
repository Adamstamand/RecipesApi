using RecipesCore.DTOs;
using RecipesCore.Entities;
using RecipesCore.Identity;

namespace RecipesCore.RepositoryContracts;

public interface IRecipesRepository
{
    Task<Recipe> AddRecipe(UserRecipe addRecipe);
    Task<bool> DoesRecipeExistAlready(Recipe recipeRequest);
    Task<Recipe[]> AllRecipes();
    Task<Recipe?> SpecificRecipe(int id);
    Task<bool> CheckRecipeAccess(int id, ApplicationUser user);
    Task<Recipe[]> UserRecipes(ApplicationUser user);
    Task<string?> UpdateRecipe(Recipe recipe);
    Task<string?> RemoveRecipe(int id);
}