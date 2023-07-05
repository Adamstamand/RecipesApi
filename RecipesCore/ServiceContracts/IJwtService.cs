using RecipesCore.DTOs;
using RecipesCore.Identity;

namespace RecipesCore.ServiceContracts;

public interface IJwtService
{
    AuthenticationResponseDTO CreateJwtToken(ApplicationUser user);
}
