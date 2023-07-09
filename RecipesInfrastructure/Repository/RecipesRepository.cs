using RecipesCore.Entities;
using Microsoft.EntityFrameworkCore;
using RecipesCore.RepositoryContracts;
using RecipesInfrastructure.Data;
using RecipesCore.DTOs;

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
        return await _context.Recipes.Include("Ingredients").Include("Instructions").ToListAsync();
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
}
