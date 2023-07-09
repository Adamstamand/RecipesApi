using Microsoft.AspNetCore.Mvc;
using RecipesCore.RepositoryContracts;
using RecipesCore.Entities;
using Microsoft.AspNetCore.Authorization;
using RecipesCore.DTOs;
using RecipesCore.ServiceContracts;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using RecipesCore.Identity;

namespace RecipesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipesRepository _recipesRepository;
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecipeController(IRecipesRepository recipesRepository,
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager
        )
    {
        _recipesRepository = recipesRepository;
        _jwtService = jwtService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IEnumerable<Recipe>> GetAllRecipes()
    {
        IEnumerable<Recipe> recipes = await _recipesRepository.AllRecipes();
        return recipes;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Recipe>> GetSpecificRecipe(int id)
    {
        Recipe recipe = await _recipesRepository.SpecificRecipe(id);
        if (recipe is null) return NotFound();
        return recipe;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostRecipe(AddRecipeDTO addRecipeRequest)
    {
        var principal = _jwtService.GetPrincipalFromJwtToken(addRecipeRequest.Token!.Token!);
        if (principal == null) return BadRequest("Invalid Jwt access token");

        string? userName = principal.FindFirstValue(JwtRegisteredClaimNames.Name);
        if (userName is null) return BadRequest("UserName not found in token");

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user == null ||
            user.RefreshToken != addRecipeRequest.Token!.RefreshToken! ||
            user.RefreshTokenExpiration <= DateTime.Now)
        {
            return BadRequest("Invalid refresh token");
        }

        UserRecipe userRecipe = new()
        {
            Recipe = addRecipeRequest.Recipe,
            User = user
        };

        var newRecipe = await _recipesRepository.AddRecipe(userRecipe);

        return CreatedAtAction(nameof(GetAllRecipes), new { id = newRecipe.RecipeId }, newRecipe);
    }
}
