using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class TokenModel
{
    [Required]
    public string? Token { get; set; }
    [Required]
    public string? RefreshToken { get; set; }
}
