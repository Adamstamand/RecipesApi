using Microsoft.Extensions.Primitives;
using RecipesCore.Identity;

namespace RecipesCore.RepositoryContracts;
public interface IUserRepository
{
    Task<ApplicationUser?> FindUserFromJwtHeader(StringValues headerValue);
}
