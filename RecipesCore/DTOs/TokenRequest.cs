using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class TokenRequest
{
    [Required]
    public required string Token { get; set; }
    [Required]
    public required string RefreshToken { get; set; }
}
