using RecipesCore.Identity;
using RecipesCore.Models;
using System.Security.Claims;

namespace RecipesCore.ServiceContracts;

public interface IJwtService
{
    NewToken CreateJwtToken(ApplicationUser user);
    ClaimsPrincipal? GetPrincipalFromJwtToken(string token);
}
