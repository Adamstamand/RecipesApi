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
    public string? Instructions { get; set; }
    [Required]
    public string? Ingredients { get; set; }
    [Required]
    public string? Photo { get; set; }
    [Required]
    public int TimeToPrepare { get; set; }
}