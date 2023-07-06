using RecipesCore.DTOs;
using RecipesCore.Identity;
using System.Security.Claims;

namespace RecipesCore.ServiceContracts;

public interface IJwtService
{
    AuthenticationResponseDTO CreateJwtToken(ApplicationUser user);
    ClaimsPrincipal? GetPrincipalFromJwtToken(string token);
}
