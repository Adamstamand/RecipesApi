namespace RecipesCore.Models;

public class NewToken
{
    public required string UserName { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}
