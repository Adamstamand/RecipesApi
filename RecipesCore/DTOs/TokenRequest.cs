using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class TokenRequest
{
    [Required, StringLength(700)]
    public required string Token { get; set; }
    [Required, StringLength(300)]
    public required string RefreshToken { get; set; }
}
