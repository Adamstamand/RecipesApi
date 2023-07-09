using RecipesCore.Validations;
using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Recipe
{
    public int RecipeId { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Description { get; set; }
    [Required]
    public ICollection<Instruction>? Instructions { get; set; }
    [Required]
    public ICollection<Ingredient>? Ingredients { get; set; }
    [Required]
    public string? Photo { get; set; }
    [Required]
    public int TimeToPrepare { get; set; }
    [Required]
    [PrivacyValidator]
    public string? Privacy { get; set; }
}