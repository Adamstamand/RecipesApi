using RecipesCore.DTOs;
using RecipesCore.Entities;
using RecipesCore.Identity;

namespace RecipesCore.RepositoryContracts;

public interface IRecipesRepository
{
    Task<Recipe> AddRecipe(UserRecipe addRecipe);
    Task<Recipe[]> AllRecipes();
    Task<Recipe?> SpecificRecipe(int id);
    Task<bool> CheckRecipeAccess(int id, ApplicationUser user);
    Task<Recipe[]> UserRecipes(ApplicationUser user);
    void UpdateRecipe();
    Task<string?> RemoveRecipe(int id);
}