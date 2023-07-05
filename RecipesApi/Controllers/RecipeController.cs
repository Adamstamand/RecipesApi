using Microsoft.AspNetCore.Mvc;
using RecipesCore.RepositoryContracts;
using RecipesCore.Entities;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize]
    public async Task<Recipe> GetSpecificRecipe(int id)
    {
        Recipe recipe = await _recipesRepository.SpecificRecipe(id);
        return recipe;
    }

    [HttpPost]
    [Authorize]
    public IActionResult PostRecipe(Recipe recipe)
    {
        _recipesRepository.AddRecipe(recipe);
        return CreatedAtAction(nameof(GetAllRecipes), new { id = recipe.RecipeId }, recipe);
    }
}
