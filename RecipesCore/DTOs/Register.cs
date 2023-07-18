using System.ComponentModel.DataAnnotations;

namespace RecipesCore.DTOs;

public class Register
{
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }  
    
    [Required, Compare("Password")]
    public required string ConfirmPassword { get; set; }
}
