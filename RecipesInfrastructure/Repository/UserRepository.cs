using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using RecipesCore.Identity;
using RecipesCore.RepositoryContracts;
using RecipesCore.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecipesInfrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(IJwtService jwtService, UserManager<ApplicationUser> userManager)
    {
        _jwtService = jwtService;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> FindUserFromJwtHeader(StringValues headerValue)
    {
        string? token = headerValue.FirstOrDefault();
        if (token is null) return null;

        string tokenNoBearer = token.Remove(0, 7);

        ClaimsPrincipal? principal = _jwtService.GetPrincipalFromJwtToken(tokenNoBearer);
        if (principal is null) return null;

        string? userName = principal.FindFirstValue(JwtRegisteredClaimNames.Name);
        if (userName is null) return null;

        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
        if (user is null) return null;

        return user;
    }
}
