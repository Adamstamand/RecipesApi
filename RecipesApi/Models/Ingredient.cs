using System.ComponentModel.DataAnnotations;

namespace RecipesApi.Models;

public class Ingredient
{
    public int Id { get; set; }
    [Required]
    public string? Words { get; set; }
}
