using RecipesCore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using RecipesCore.RepositoryContracts;
using RecipesInfrastructure.Data;

namespace RecipesInfrastructure.Repository;

public class RecipesRepository : IRecipesRepository
{
    private readonly ApplicationDbContext _context;

    public RecipesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async void AddRecipe(Recipe recipe)
    {
        await _context.Recipes.AddAsync(recipe);
        _context.SaveChanges();
    }

    public async Task<IEnumerable<Recipe>> AllRecipes()
    {
        return await _context.Recipes.Include("Ingredients").Include("Instructions").ToListAsync();
    }

    public async Task<Recipe> SpecificRecipe(int id)
    {
        return await _context.Recipes.Include("Ingredients").Include("Instructions").FirstAsync(recipe => recipe.RecipeId == id);
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
