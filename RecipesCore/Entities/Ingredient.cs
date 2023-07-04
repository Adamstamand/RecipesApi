using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Ingredient
{
    public int Id { get; set; }
    [Required]
    public string? Words { get; set; }
}
