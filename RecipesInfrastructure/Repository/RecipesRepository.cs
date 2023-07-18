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
        _context.Recipes.Add(addRecipe.Recipe);
        _context.UserRecipes.Add(addRecipe);
        await _context.SaveChangesAsync();

        return addRecipe.Recipe;
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
                .FirstAsync(recipe => recipe.Id == id);
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
            .Where(recipe => recipe.Id == id)
            .Include("Ingredients")
            .Include("Instructions")
            .Join(_context.UserRecipes,
                recipe => recipe.Id,
                userRecipe => userRecipe.RecipeId,
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
        _context.Recipes.Update(recipe);

        int[] newIngredientIds = recipe.Ingredients
            .Select(ingredient => ingredient.Id)
            .ToArray();

        int[] ingredientIdsThatMatchThisRecipe = await _context.Ingredients
            .Where(ingredient => ingredient.RecipeId == recipe.Id)
            .Select(ingredient => ingredient.Id)
            .ToArrayAsync();

        int[] ingredientsDeletedOnClientSide = ingredientIdsThatMatchThisRecipe
            .Except(newIngredientIds)
            .ToArray();

        if (ingredientsDeletedOnClientSide.Length > 0)
        {
            Ingredient[] ingredientsToRemove = await _context.Ingredients
                .Where(ingredient =>
                    ingredientsDeletedOnClientSide.Contains(ingredient.Id))
                .ToArrayAsync();

            _context.Ingredients.RemoveRange(ingredientsToRemove);
        }

        int[] newInstructionIds = recipe.Instructions
            .Select(instruction => instruction.Id)
            .ToArray();

        int[] instructionIdsThatMatchThisRecipe = await _context.Instructions
            .Where(instruction => instruction.RecipeId == recipe.Id)
            .Select(instruction => instruction.Id)
            .ToArrayAsync();

        int[] instructionsDeletedOnClientSide = instructionIdsThatMatchThisRecipe
            .Except(newInstructionIds)
            .ToArray();

        if (instructionsDeletedOnClientSide.Length > 0)
        {
            Instruction[] instructionsToRemove = await _context.Instructions
                .Where(instruction =>
                instructionsDeletedOnClientSide.Contains(instruction.Id))
                .ToArrayAsync();

            _context.Instructions.RemoveRange(instructionsToRemove);
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
        userRecipe.Recipe!.Id == id && userRecipe.User!.Id == user.Id);
        return didUserMakeThisRecipe;
    }

    public async Task<Recipe[]> UserRecipes(ApplicationUser user)
    {
        return await _context.UserRecipes
            .Where(recipe => recipe.User!.Id == user.Id)
            .Join(_context.Recipes,
                userRecipe => userRecipe.Recipe!.Id,
                recipe => recipe.Id,
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
