namespace RecipesCore.DTOs;

public class AuthenticationResponseDTO
{
    public string? UserName { get; set; }      
    public string? Token { get; set; }      
    public DateTime? Expiration { get; set; }
    public string? RefreshToken { get; set;}
    public DateTime RefreshTokenExpiration { get; set;}
}