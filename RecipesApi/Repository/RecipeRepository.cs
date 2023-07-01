using RecipesApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using RecipesApi.Interfaces;
using RecipesApi.Data;

namespace RecipesApi.Repository;

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
        return await _context.Recipes.ToListAsync();
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
