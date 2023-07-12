using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using RecipesCore.Entities;
using RecipesCore.Identity;
using RecipesCore.RepositoryContracts;
using RecipesCore.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecipesApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{

    private readonly IRecipesRepository _recipesRepository;
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IRecipesRepository recipesRepository,
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager
        )
    {
        _recipesRepository = recipesRepository;
        _jwtService = jwtService;
        _userManager = userManager;
    }


    [HttpGet]
    [Authorize]
    public async Task<ActionResult<Recipe>> GetDashboard()
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues headerValue))
        {
            return BadRequest("Invalid request");
        }
        var token = headerValue.FirstOrDefault();
        if (token is null) return BadRequest();

        var tokenNoBearer = token.Remove(0, 7);
        var principal = _jwtService.GetPrincipalFromJwtToken(tokenNoBearer);
        if (principal is null) return BadRequest();

        string? userName = principal.FindFirstValue(JwtRegisteredClaimNames.Name);
        if (userName is null) return BadRequest("UserName not found");

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user is null) return BadRequest("Invalid UserName");

        var userRecipes = await _recipesRepository.UserRecipes(user);

        return Ok(userRecipes);
    }
}
