using RecipesCore.Entities;
using Microsoft.EntityFrameworkCore;
using RecipesCore.RepositoryContracts;
using RecipesInfrastructure.Data;
using RecipesCore.DTOs;
using RecipesCore.Identity;
using System.Linq;

namespace RecipesInfrastructure.Repository;

public class RecipesRepository : IRecipesRepository
{
    private readonly ApplicationDbContext _context;

    public RecipesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Recipe> AddRecipe(UserRecipe addRecipe)
    {
        await _context.Recipes.AddAsync(addRecipe.Recipe!);
        await _context.UserRecipes.AddAsync(addRecipe);
        _context.SaveChanges();

        return addRecipe.Recipe!;
    }

    public async Task<IEnumerable<Recipe>> AllRecipes()
    {
        return await _context.Recipes
            .Where(recipe => recipe.Privacy != "private")
            .Include("Ingredients")
            .Include("Instructions")
            .ToListAsync();
    }

    public async Task<Recipe> SpecificRecipe(int id)
    {
        Recipe specificRecipe;
        try
        {
            specificRecipe = await _context.Recipes.Include("Ingredients").Include("Instructions").FirstAsync(recipe => recipe.RecipeId == id);
        }
        catch
        {
            return null!;
        }
        return specificRecipe;
    }

    public void RemoveRecipe()
    {
        throw new NotImplementedException();
    }

    public void UpdateRecipe()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CheckRecipeAccess(int id, ApplicationUser user)
    {
        var didUserMakeThisRecipe = await _context.UserRecipes.AnyAsync(userRecipe =>
        userRecipe.Recipe!.RecipeId == id && userRecipe.User!.Id == user.Id);
        return didUserMakeThisRecipe;
    }

    public async Task<Recipe[]> UserRecipes(ApplicationUser user)
    {
        var userRecipeIds = await _context.UserRecipes
            .Where(recipe => recipe.User!.Id == user.Id)
            .Select(userRecipe => userRecipe.Recipe!.RecipeId)
            .ToArrayAsync();
        var userRecipes = await _context.Recipes
            .Where(recipes => userRecipeIds.Contains(recipes.RecipeId))
            .Include("Ingredients").Include("Instructions")
            .ToArrayAsync();
        return userRecipes;
        
    }
}
