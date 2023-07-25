using Microsoft.AspNetCore.Mvc;
using RecipesCore.RepositoryContracts;
using RecipesCore.Entities;
using Microsoft.AspNetCore.Authorization;
using RecipesCore.ServiceContracts;
using RecipesCore.Identity;
using Microsoft.Extensions.Primitives;

namespace RecipesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly IUserRepository _userRepository;

    public RecipeController(IRecipesRepository recipesRepository,
        IUserRepository userRepository)
    {
        _recipesRepository = recipesRepository;
        _userRepository = userRepository;
    }


    [HttpGet]
    public async Task<Recipe[]> GetAllRecipes()
    {
        Recipe[] recipes = await _recipesRepository.AllRecipes();
        return recipes;
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<Recipe>> GetSpecificRecipe(int id)
    {
        Recipe? recipe = await _recipesRepository.SpecificRecipe(id);
        if (recipe is null) return NotFound("That recipe doesn't exist");
        if (recipe.Privacy == "public") return recipe;

        if (!Request.Headers.TryGetValue("Authorization", out StringValues headerValue))
        {
            return Unauthorized("Invalid token");
        }

        ApplicationUser? user = await _userRepository.FindUserFromJwtHeader(headerValue);
        if (user is null) return Unauthorized("User not found");

        bool isAccessGranted = await _recipesRepository.CheckRecipeAccess(id, user);
        if (isAccessGranted) return recipe;

        return Unauthorized("Only the user who created a private recipe can access it");
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostRecipe(Recipe addRecipeRequest)
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues headerValue))
        {
            return Unauthorized("Invalid token");
        }

        ApplicationUser? user = await _userRepository.FindUserFromJwtHeader(headerValue);
        if (user is null) return Unauthorized("User not found");

        bool isThereAlreadyARecipeWithThatName = await _recipesRepository
            .DoesRecipeExistAlready(addRecipeRequest);

        if (isThereAlreadyARecipeWithThatName)
        {
            return BadRequest("A recipe with that name already exists");
        }

        UserRecipe userRecipe = new()
        {
            Recipe = addRecipeRequest,
            User = user
        };

        var newRecipe = await _recipesRepository.AddRecipe(userRecipe);

        return CreatedAtAction(nameof(GetAllRecipes), new { id = newRecipe.Id }, newRecipe);
    }


    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues headerValue))
        {
            return Unauthorized("Invalid token");
        }

        ApplicationUser? user = await _userRepository.FindUserFromJwtHeader(headerValue);
        if (user is null) return Unauthorized("User not found");

        bool isAccessGranted = await _recipesRepository.CheckRecipeAccess(id, user);
        if (!isAccessGranted)
        {
            return Unauthorized("Only the user who created the recipe can delete it");
        }

        string? isRecipeDeleted = await _recipesRepository.RemoveRecipe(id);
        if (isRecipeDeleted == "deleted") return NoContent();

        return BadRequest("Recipe not found");
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> PutRecipe(int id, Recipe recipe)
    {
        if (id != recipe.Id) return BadRequest("The IDs don't match");

        if (!Request.Headers.TryGetValue("Authorization", out StringValues headerValue))
        {
            return Unauthorized("Invalid token");
        }

        ApplicationUser? user = await _userRepository.FindUserFromJwtHeader(headerValue);
        if (user is null) return Unauthorized("User not found");

        bool isAccessGranted = await _recipesRepository.CheckRecipeAccess(recipe.Id, user);
        if (!isAccessGranted)
        {
            return Unauthorized("Only the user who created the recipe can update it");
        }

        string? isRecipeUpdated = await _recipesRepository.UpdateRecipe(recipe);
        if (isRecipeUpdated == "updated") return NoContent();
        
        return BadRequest();
    }
}