using RecipesCore.Validations;
using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Recipe
{
    public int Id { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Description { get; set; }
    [Required]
    public required ICollection<Instruction> Instructions { get; set; }
    [Required]
    public required ICollection<Ingredient> Ingredients { get; set; }
    public string? Photo { get; set; }
    [Required]
    public int TimeToPrepare { get; set; }
    [Required, PrivacyValidator]
    public required string Privacy { get; set; }
}