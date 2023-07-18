using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class LogIn
{
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
}
