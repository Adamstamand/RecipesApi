using Microsoft.AspNetCore.Mvc;
using RecipesApi.Interfaces;
using RecipesApi.Models;

namespace RecipesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipesRepository _recipesRepository;


    public RecipeController(IRecipesRepository recipesRepository)
    {
        _recipesRepository = recipesRepository;
    }

    [HttpGet]
    public async Task<IEnumerable<Recipe>> GetAllRecipes()
    {
        IEnumerable<Recipe> recipes = await _recipesRepository.AllRecipes();
        return recipes;
    }

    [HttpGet("{id}")]
    public async Task<Recipe> GetSpecificRecipe(int id)
    {
        Recipe recipe = await _recipesRepository.SpecificRecipe(id);
        return recipe;
    }

    [HttpPost]
    public IActionResult PostRecipe(Recipe recipe)
    {
        _recipesRepository.AddRecipe(recipe);
        return CreatedAtAction(nameof(GetAllRecipes), new { id = recipe.RecipeId }, recipe);
    }
}
