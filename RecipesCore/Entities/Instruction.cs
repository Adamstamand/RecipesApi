using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Instruction
{
    public int Id { get; set; }
    [Required]
    public required string Words { get; set; }
    [Required]
    public int Position { get; set; }
    public int RecipeId { get; set; }
}
