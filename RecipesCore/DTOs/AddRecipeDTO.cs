using RecipesCore.Entities;
using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class AddRecipeDTO
{
    [Required]
    public TokenModel? Token { get; set; }
    [Required]
    public Recipe? Recipe { get; set; }
}
