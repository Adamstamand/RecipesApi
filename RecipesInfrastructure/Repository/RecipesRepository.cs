using RecipesCore.Entities;
using Microsoft.EntityFrameworkCore;
using RecipesCore.RepositoryContracts;
using RecipesInfrastructure.Data;
using RecipesCore.Identity;

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
        await _context.SaveChangesAsync();

        return addRecipe.Recipe!;
    }

    public async Task<Recipe[]> AllRecipes()
    {
        return await _context.Recipes
            .Where(recipe => recipe.Privacy != "private")
            .Include("Ingredients")
            .Include("Instructions")
            .ToArrayAsync();
    }

    public async Task<Recipe?> SpecificRecipe(int id)
    {
        Recipe specificRecipe;
        try
        {
            specificRecipe = await _context.Recipes
                .Include("Ingredients")
                .Include("Instructions")
                .FirstAsync(recipe => recipe.RecipeId == id);
        }
        catch
        {
            return null;
        }
        return specificRecipe;
    }

    public async Task<string?> RemoveRecipe(int id)
    {

        var recipeToDelete = await _context.Recipes
            .Where(recipe => recipe.RecipeId == id)
            .Include("Ingredients")
            .Include("Instructions")
            .Join(_context.UserRecipes,
                recipe => recipe.RecipeId,
                userRecipe => userRecipe.Recipe!.RecipeId,
                (recipe, userRecipe) => new { RecipeValue = recipe, UserRecipeValue = userRecipe })
            .FirstOrDefaultAsync();

        if (recipeToDelete is null) return null;
        Recipe recipeName = recipeToDelete.RecipeValue;
        _context.Recipes.Remove(recipeName);

        UserRecipe userRecipeName = recipeToDelete.UserRecipeValue;
        _context.UserRecipes.Remove(userRecipeName);
        int? amountOfValuesUpdated = 0;
        try
        {
            amountOfValuesUpdated = await _context.SaveChangesAsync();
        }
        catch
        {
            return null;
        }
        if (amountOfValuesUpdated < 1) return null;
        return "deleted";
    }

    public async Task<string?> UpdateRecipe(Recipe recipe)
    {
        _context.Recipes.Entry(recipe).State = EntityState.Modified;

        if (recipe.Ingredients is not null)
        {
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                if (_context.Ingredients.Any(value => value.Id == ingredient.Id))
                {
                    _context.Ingredients.Entry(ingredient).State = EntityState.Modified;
                }
                else
                {
                    _context.Add(ingredient);
                }    
            }
        }

        if (recipe.Instructions is not null)
        {
            foreach (Instruction instruction in recipe.Instructions)
            {
                if (_context.Instructions.Any(value => value.Id == instruction.Id))
                {
                    _context.Instructions.Entry(instruction).State = EntityState.Modified;
                }
                else
                {
                    _context.Add(instruction);
                }  
            }
        }

        int amountOfValuesUpdated;
        try
        {
            amountOfValuesUpdated = await _context.SaveChangesAsync();
        }
        catch
        {
            return null;
        }
        if (amountOfValuesUpdated < 1) return null;
        return "updated";
        
        
    }

    public async Task<bool> CheckRecipeAccess(int id, ApplicationUser user)
    {
        bool didUserMakeThisRecipe = await _context.UserRecipes.AnyAsync(userRecipe =>
        userRecipe.Recipe!.RecipeId == id && userRecipe.User!.Id == user.Id);
        return didUserMakeThisRecipe;
    }

    public async Task<Recipe[]> UserRecipes(ApplicationUser user)
    {
        return await _context.UserRecipes
            .Where(recipe => recipe.User!.Id == user.Id)
            .Join(_context.Recipes,
                userRecipe => userRecipe.Recipe!.RecipeId,
                recipe => recipe.RecipeId,
                (userRecipe, recipe) => recipe)
            .Include("Ingredients")
            .Include("Instructions")
            .ToArrayAsync();
    }

    public Task<bool> DoesRecipeExistAlready(Recipe recipeRequest)
    {
        return _context.Recipes
            .AnyAsync(recipe => recipe.Name == recipeRequest.Name);
    }
}
