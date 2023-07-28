using RecipesCore.Validations;
using System.ComponentModel.DataAnnotations;

namespace RecipesCore.Entities;

public class Recipe
{
    public int Id { get; set; }
    [Required, StringLength(40)]
    public required string Name { get; set; }
    [Required, StringLength(1000)]
    public required string Description { get; set; }
    [Required]
    public required ICollection<Instruction> Instructions { get; set; }
    [Required]
    public required ICollection<Ingredient> Ingredients { get; set; }
    [StringLength(500), RegularExpression(@"^https:\/\/images\.unsplash\.com\/.*", ErrorMessage = "Incorrect photo url")]
    public string? Photo { get; set; }
    [Required, Range(0, 1440)]
    public int TimeToPrepare { get; set; }
    [Required, PrivacyValidator]
    public required string Privacy { get; set; }
}