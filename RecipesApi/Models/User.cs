using System.ComponentModel.DataAnnotations;

namespace RecipesApi.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? PasswordSalt { get; set; }
    [Required]
    public string? PasswordHash { get; set; }
    public ICollection<Recipe>? Recipes { get; set; }
}