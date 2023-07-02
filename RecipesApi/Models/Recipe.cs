using System.ComponentModel.DataAnnotations;

namespace RecipesApi.Models;

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
}