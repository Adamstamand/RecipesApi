using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using RecipesCore.Entities;
using RecipesCore.Identity;
using RecipesCore.RepositoryContracts;

namespace RecipesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{

    private readonly IRecipesRepository _recipesRepository;
    private readonly IUserRepository _userRepository;

    public DashboardController(IRecipesRepository recipesRepository,
        IUserRepository userRepository)
    {
        _recipesRepository = recipesRepository;
        _userRepository = userRepository;
    }


    [HttpGet]
    [Authorize]
    public async Task<ActionResult<Recipe>> GetDashboard()
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues headerValue))
        {
            return Unauthorized("Invalid token");
        }

        ApplicationUser? user = await _userRepository.FindUserFromJwtHeader(headerValue);
        if (user is null) return Unauthorized("User not found");

        Recipe[] userRecipes = await _recipesRepository.UserRecipes(user);
        if (!userRecipes.Any())
        {
            return Ok(new string[1] {"No recipes"});
        }
        return Ok(userRecipes);
    }
}
