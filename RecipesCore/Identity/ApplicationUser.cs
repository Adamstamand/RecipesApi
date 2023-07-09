using Microsoft.AspNetCore.Identity;
using RecipesCore.Entities;

namespace RecipesCore.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}